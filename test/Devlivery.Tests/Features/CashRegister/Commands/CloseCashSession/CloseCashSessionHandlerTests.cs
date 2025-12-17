using Devlivery.Features.CashRegister.Commands.CloseCashSession;
using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CloseCashSession;

public sealed class CloseCashSessionHandlerTests : IClassFixture<CashRegisterUnitTestFixture>
{
    private readonly CashRegisterUnitTestFixture _fixture;
    private readonly ICashSessionRepository _cashSessionRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CloseCashSessionHandler _handler;

    public CloseCashSessionHandlerTests(CashRegisterUnitTestFixture fixture)
    {
        _fixture = fixture;
        _cashSessionRepository = Substitute.For<ICashSessionRepository>();
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CloseCashSessionHandler(_cashSessionRepository, _orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_Should_Close_Cash_Session_Successfully()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession(openingAmount: 100m);
        var cashSessionId = cashSession.Id;
        const decimal closingAmount = 250m;
        const string notes = "Fechamento normal";

        var orders = new List<Order>
        {
            CreateTestOrder(50m, PaymentMethod.Cash),
            CreateTestOrder(30m, PaymentMethod.Cash),
            CreateTestOrder(70m, PaymentMethod.CreditCard)
        };

        _cashSessionRepository.GetByIdAsync(cashSessionId, Arg.Any<CancellationToken>())
            .Returns(cashSession);
        _orderRepository.GetOrdersInPeriodAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(orders);

        var command = new CloseCashSessionCommand(cashSessionId, closingAmount, notes);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Status.ShouldBe("Closed");
        result.Value.ClosingAmount.ShouldBe(closingAmount);
        result.Value.TotalRevenue.ShouldBe(150m);
        result.Value.TotalOrders.ShouldBe(3);

        await _cashSessionRepository.Received(1).UpdateAsync(cashSession, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Cash_Session_Not_Found()
    {
        // Arrange
        var cashSessionId = Guid.NewGuid();
        var command = new CloseCashSessionCommand(cashSessionId, 100m, null);

        _cashSessionRepository.GetByIdAsync(cashSessionId, Arg.Any<CancellationToken>())
            .Returns((CashSession?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        var error = result.Errors[0];
        error.Metadata["Errors"].ShouldBe(new[] { "Caixa não encontrado." });
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Cash_Session_Already_Closed()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession();
        cashSession.Close(100m, null);

        _cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);

        var command = new CloseCashSessionCommand(cashSession.Id, 100m, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        var error = result.Errors[0];
        error.Metadata["Errors"].ShouldBe(new[] { "O caixa já está fechado." });
    }

    [Fact]
    public async Task Handle_Should_Calculate_Payment_Breakdown_Correctly()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession(openingAmount: 100m);

        var orders = new List<Order>
        {
            CreateTestOrder(50m, PaymentMethod.Cash),
            CreateTestOrder(30m, PaymentMethod.Cash),
            CreateTestOrder(70m, PaymentMethod.CreditCard),
            CreateTestOrder(20m, PaymentMethod.Pix)
        };

        _cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);
        _orderRepository.GetOrdersInPeriodAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(orders);

        var command = new CloseCashSessionCommand(cashSession.Id, 180m, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.PaymentBreakdown.Count.ShouldBe(3);
        result.Value.PaymentBreakdown.First(p => p.Method == "Cash").Amount.ShouldBe(80m);
        result.Value.PaymentBreakdown.First(p => p.Method == "CreditCard").Amount.ShouldBe(70m);
        result.Value.PaymentBreakdown.First(p => p.Method == "Pix").Amount.ShouldBe(20m);
    }

    [Fact]
    public async Task Handle_Should_Calculate_Expected_Cash_Amount_With_Deposits()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession(openingAmount: 100m);

        // Add deposits
        var deposit1 = _fixture.CreateCashDeposit(cashSessionId: cashSession.Id, amount: 50m);
        var deposit2 = _fixture.CreateCashDeposit(cashSessionId: cashSession.Id, amount: 30m);
        cashSession.AddDeposit(deposit1);
        cashSession.AddDeposit(deposit2);

        var orders = new List<Order>
        {
            CreateTestOrder(40m, PaymentMethod.Cash), CreateTestOrder(60m, PaymentMethod.Cash)
        };

        _cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);
        _orderRepository.GetOrdersInPeriodAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(orders);

        var command = new CloseCashSessionCommand(cashSession.Id, 280m, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        // Expected: Opening (100) + Deposits (50 + 30) + Cash Sales (40 + 60) = 280
        result.Value.ExpectedCashAmount.ShouldBe(280m);
    }

    [Fact]
    public async Task Handle_Should_Close_Session_With_No_Orders()
    {
        // Arrange
        var cashSession = _fixture.CreateCashSession(openingAmount: 100m);
        var orders = new List<Order>();

        _cashSessionRepository.GetByIdAsync(cashSession.Id, Arg.Any<CancellationToken>())
            .Returns(cashSession);
        _orderRepository.GetOrdersInPeriodAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(orders);

        var command = new CloseCashSessionCommand(cashSession.Id, 100m, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalRevenue.ShouldBe(0m);
        result.Value.TotalOrders.ShouldBe(0);
        result.Value.ExpectedCashAmount.ShouldBe(100m);
    }

    private static Order CreateTestOrder(decimal total, PaymentMethod paymentMethod)
    {
        // Criar OrderItem com valor igual ao total (sem taxa de entrega)
        var establishmentId = Guid.NewGuid();
        var productBuilder = new ProductBuilder().WithPrice(total).WithEstablishmentId(establishmentId);
        var product = productBuilder.Build();

        var orderItem = new OrderItemBuilder()
            .WithQuantity(1)
            .WithProduct(product)
            .WithEstablishmentId(establishmentId)
            .Build();

        var order = new OrderBuilder()
            .WithEstablishmentId(establishmentId)
            .WithPaymentMethod(paymentMethod)
            .WithDeliveryFee(0m)
            .WithItems(orderItem)
            .Build();

        return order;
    }
}