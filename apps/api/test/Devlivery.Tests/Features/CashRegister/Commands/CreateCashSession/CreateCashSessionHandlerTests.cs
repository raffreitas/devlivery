using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.CashRegister;
using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Features.CashRegister.Commands.CreateCashSession;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Tests.Common.Builders;
using Devlivery.Tests.Common.Mocks;

using NSubstitute;
using NSubstitute.ReturnsExtensions;

using Shouldly;

namespace Devlivery.Tests.Features.CashRegister.Commands.CreateCashSession;

[Trait("Category", "Unit Tests")]
public sealed class CreateCashSessionHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _operatorId = Guid.NewGuid();
    private readonly ICashSessionRepository _cashSessionRepository = Substitute.For<ICashSessionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateCashSessionHandler _handler;

    public CreateCashSessionHandlerTests()
    {
        _handler = new CreateCashSessionHandler(
            _cashSessionRepository,
            TestMocks.CurrentUserAccessor(_operatorId, "Maria Souza"),
            _unitOfWork,
            TestMocks.TenantAccessor(_tenantId));
    }

    [Fact]
    public async Task GivenNoOpenSession_WhenOpening_ThenPersistsSessionForCurrentOperatorAndTenant()
    {
        // Arrange
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>()).ReturnsNull();
        CashSession? addedSession = null;
        await _cashSessionRepository.AddAsync(Arg.Do<CashSession>(s => addedSession = s),
            Arg.Any<CancellationToken>());

        // Act
        var result = await _handler.HandleAsync(new CreateCashSessionCommand(100m, "Turno da noite"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        addedSession.ShouldNotBeNull();
        addedSession.EstablishmentId.ShouldBe(_tenantId);
        addedSession.AttendantId.ShouldBe(_operatorId);
        addedSession.AttendantName.ShouldBe("Maria Souza");
        addedSession.OpeningAmount.ShouldBe(100m);
        addedSession.Notes.ShouldBe("Turno da noite");
        result.Value.ShouldBe(new CreateCashSessionResponse(addedSession.Id, "Maria Souza", 100m,
            addedSession.StartAt, nameof(CashSessionStatus.Open)));
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenOpenSession_WhenOpeningAnother_ThenReturnsValidationErrorWithoutPersisting()
    {
        // Arrange
        _cashSessionRepository.GetActiveSessionAsync(Arg.Any<CancellationToken>())
            .Returns(new CashSessionBuilder().Build());

        // Act
        var result = await _handler.HandleAsync(new CreateCashSessionCommand(100m, null), CancellationToken.None);

        // Assert
        result.HasError<ValidationError>().ShouldBeTrue();
        await _cashSessionRepository.DidNotReceive().AddAsync(Arg.Any<CashSession>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}