using System.Net;
using System.Net.Http.Json;

using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Products;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.Orders.Commands.CreateOrder;
using Devlivery.Features.Orders.Queries.GetAllOrders;
using Devlivery.Features.Orders.Queries.GetOrderById;
using Devlivery.Tests.Common.Api;
using Devlivery.Tests.Common.Builders;

using Microsoft.EntityFrameworkCore;

using Shouldly;

using CreateItem = Devlivery.Features.Orders.Commands.CreateOrder.OrderItemDto;
using CreatePayment = Devlivery.Features.Orders.Commands.CreateOrder.OrderPaymentDto;

namespace Devlivery.Tests.Features.Orders.Api;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration Tests")]
public sealed class OrdersApiTests(ApiFactory api)
{
    [Fact]
    public async Task GivenOrders_WhenListing_ThenReturnsNewestFirstWithProductsAndActivePaymentsOnly()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza Calabresa", 40m);
        var soda = await SeedProductAsync(tenant, "Refrigerante", 5m);
        var older = await SeedOrderAsync(tenant, [(pizza, 1)], customer: "Cliente Antigo");
        var newer = await SeedOrderAsync(tenant, [(pizza, 1), (soda, 3)], customer: "Cliente Novo",
            payments:
            [
                new OrderPaymentBuilder().WithEstablishmentId(tenant.EstablishmentId)
                    .WithPaymentMethod(PaymentMethod.Pix).WithAmount(55m).Build(),
                new OrderPaymentBuilder().WithEstablishmentId(tenant.EstablishmentId)
                    .WithPaymentMethod(PaymentMethod.Cash).WithAmount(10m).AsCancelled().Build()
            ]);
        await SetCreatedAtAsync(tenant, older, new DateTime(2026, 10, 13, 15, 0, 0, DateTimeKind.Utc));
        await SetCreatedAtAsync(tenant, newer, new DateTime(2026, 10, 14, 15, 0, 0, DateTimeKind.Utc));

        // Act
        var response = await tenant.Client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var orders = await ApiTenant.ReadDataAsync<List<GetAllOrdersResponse>>(response);
        orders.Select(o => o.CustomerName).ShouldBe(["Cliente Novo", "Cliente Antigo"]);
        var listed = orders[0];
        listed.Items.Select(i => (i.Product.Name, i.Quantity)).ShouldBe([("Refrigerante", 3), ("Pizza Calabresa", 1)]);
        listed.Total.ShouldBe(55m);
        listed.Status.ShouldBe(nameof(OrderStatus.Pending));
        listed.Payments.ShouldHaveSingleItem().PaymentMethod.ShouldBe(nameof(PaymentMethod.Pix));
    }

    [Fact]
    public async Task GivenOrdersAroundMidnightInBrazil_WhenFilteringByDay_ThenUsesSaoPauloCalendarDay()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 40m);
        var lateOnThirteenth = await SeedOrderAsync(tenant, [(pizza, 1)], customer: "Dia 13 à noite");
        var earlyOnFourteenth = await SeedOrderAsync(tenant, [(pizza, 1)], customer: "Dia 14 de madrugada");
        await SetCreatedAtAsync(tenant, lateOnThirteenth, new DateTime(2026, 10, 14, 2, 59, 0, DateTimeKind.Utc));
        await SetCreatedAtAsync(tenant, earlyOnFourteenth, new DateTime(2026, 10, 14, 3, 0, 0, DateTimeKind.Utc));

        // Act
        var response = await tenant.Client.GetAsync("/api/orders?start=2026-10-14&end=2026-10-14");

        // Assert
        var orders = await ApiTenant.ReadDataAsync<List<GetAllOrdersResponse>>(response);
        orders.ShouldHaveSingleItem().CustomerName.ShouldBe("Dia 14 de madrugada");
    }

    [Fact]
    public async Task GivenPaymentMethodFilter_WhenListing_ThenReturnsOnlyOrdersPaidWithIt()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 40m);
        await SeedOrderAsync(tenant, [(pizza, 1)], customer: "Pagou no Pix", payments:
        [
            new OrderPaymentBuilder().WithEstablishmentId(tenant.EstablishmentId)
                .WithPaymentMethod(PaymentMethod.Pix).WithAmount(40m).Build()
        ]);
        await SeedOrderAsync(tenant, [(pizza, 1)], customer: "Pagou em dinheiro", payments:
        [
            new OrderPaymentBuilder().WithEstablishmentId(tenant.EstablishmentId)
                .WithPaymentMethod(PaymentMethod.Cash).WithAmount(40m).Build()
        ]);

        // Act
        var response = await tenant.Client.GetAsync("/api/orders?paymentMethod=Pix");

        // Assert
        var orders = await ApiTenant.ReadDataAsync<List<GetAllOrdersResponse>>(response);
        orders.ShouldHaveSingleItem().CustomerName.ShouldBe("Pagou no Pix");
    }

    [Fact]
    public async Task GivenExistingOrder_WhenGettingById_ThenReturnsItemsAndPayments()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza Calabresa", 40m);
        var order = await SeedOrderAsync(tenant, [(pizza, 2)], customer: "Maria Souza");

        // Act
        var response = await tenant.Client.GetAsync($"/api/orders/{order.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await ApiTenant.ReadDataAsync<GetOrderByIdResponse>(response);
        body.CustomerName.ShouldBe("Maria Souza");
        body.Items.ShouldHaveSingleItem().Quantity.ShouldBe(2);
        body.Total.ShouldBe(80m);
        body.Payments.ShouldHaveSingleItem().Amount.ShouldBe(80m);
    }

    [Fact]
    public async Task GivenUnknownId_WhenGettingById_ThenReturns404()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();

        // Act
        var response = await tenant.Client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GivenAvailableProducts_WhenCreatingOrder_ThenReturns201AndPersistsPricedOrder()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza Calabresa", 40m);
        var command = new CreateOrderCommand(
            Items: [new CreateItem(pizza.Id, 2, "Sem cebola")],
            CustomerName: "Maria Souza",
            CustomerPhone: "11987654321",
            DeliveryAddress: "Rua das Flores, 123",
            Payments: [new CreatePayment(PaymentMethod.Cash, 100m)],
            DeliveryFee: 5m);

        // Act
        var response = await tenant.Client.PostAsJsonAsync("/api/orders", command, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await ApiTenant.ReadDataAsync<CreateOrderResponse>(response);
        var persisted = await LoadOrderAsync(tenant, created.OrderId);
        persisted.Total.ShouldBe(85m);
        persisted.Items.ShouldHaveSingleItem().UnitPrice.ShouldBe(40m);
        persisted.Payments.ShouldHaveSingleItem().Amount.ShouldBe(100m);
    }

    [Fact]
    public async Task GivenOpenOrder_WhenUpdating_ThenReturns204AndPersistsNewDetails()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza Calabresa", 40m);
        var order = await SeedOrderAsync(tenant, [(pizza, 1)], customer: "Maria Souza");
        var payment = order.Payments.Single();
        var request = new
        {
            Items = new[] { new { ProductId = pizza.Id, Quantity = 2, Notes = (string?)null } },
            CustomerName = "Maria Souza",
            CustomerPhone = (string?)null,
            DeliveryAddress = "Av. Brasil, 500",
            Payments = new[] { new { Id = (Guid?)payment.Id, Method = PaymentMethod.Pix, Amount = 80m } },
            DeliveryReference = (string?)null,
            DeliveryFee = 0m
        };

        // Act
        var response = await tenant.Client.PutAsJsonAsync($"/api/orders/{order.Id}", request, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var persisted = await LoadOrderAsync(tenant, order.Id);
        persisted.DeliveryAddress.FullAddress.ShouldBe("Av. Brasil, 500");
        persisted.Items.ShouldHaveSingleItem().Quantity.ShouldBe(2);
        persisted.Total.ShouldBe(80m);
        var persistedPayment = persisted.Payments.ShouldHaveSingleItem();
        persistedPayment.PaymentMethod.ShouldBe(PaymentMethod.Pix);
        persistedPayment.Amount.ShouldBe(80m);
    }

    [Fact]
    public async Task GivenOpenCashSession_WhenDeliveringCashOrder_ThenRecordsPaymentAndChangeForOperator()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 70m);
        var order = await SeedOrderAsync(tenant, [(pizza, 1)], customer: "Maria Souza", payments:
        [
            new OrderPaymentBuilder().WithEstablishmentId(tenant.EstablishmentId)
                .WithPaymentMethod(PaymentMethod.Cash).WithAmount(100m).Build()
        ]);
        var session = new CashSessionBuilder().WithEstablishmentId(tenant.EstablishmentId)
            .WithAttendantId(tenant.Operator.Id).WithOpeningAmount(50m).Build();
        await tenant.SeedAsync(session);

        // Act
        var response = await tenant.Client.PatchAsJsonAsync($"/api/orders/{order.Id}/status",
            new { Status = OrderStatus.Delivered }, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await LoadOrderAsync(tenant, order.Id)).Status.ShouldBe(OrderStatus.Delivered);
        var movements = await tenant.WithDbContextAsync(db =>
            db.CashSessionMovements.Where(m => m.CashSessionId == session.Id).ToListAsync());
        movements.Select(m => (m.EntryType, m.Amount)).ShouldBe(
            [(CashSessionEntryType.Payment, 100m), (CashSessionEntryType.Change, 30m)], ignoreOrder: true);
        movements.ShouldAllBe(m => m.CreatedBy == tenant.Operator.Id);
    }

    [Fact]
    public async Task GivenDeliveredOrder_WhenMovingBackToPreparing_ThenReturns422WithRuleMessage()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 40m);
        var order = await SeedOrderAsync(tenant, [(pizza, 1)], customer: "Maria Souza",
            status: OrderStatus.Delivered);

        // Act
        var response = await tenant.Client.PatchAsJsonAsync($"/api/orders/{order.Id}/status",
            new { Status = OrderStatus.Preparing }, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await ApiTenant.ReadProblemAsync(response)).Detail
            .ShouldBe("Não é possível alterar o status de um pedido já entregue.");
        (await LoadOrderAsync(tenant, order.Id)).Status.ShouldBe(OrderStatus.Delivered);
    }

    [Fact]
    public async Task GivenOrderDeliveredInOpenSession_WhenDeleting_ThenReturns204AndRefundsItsPayments()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 40m);
        var order = await SeedOrderAsync(tenant, [(pizza, 1)], customer: "Maria Souza");
        var session = new CashSessionBuilder().WithEstablishmentId(tenant.EstablishmentId)
            .WithAttendantId(tenant.Operator.Id).WithOpeningAmount(0m).Build();
        await tenant.SeedAsync(session);
        await tenant.Client.PatchAsJsonAsync($"/api/orders/{order.Id}/status",
            new { Status = OrderStatus.Delivered }, ApiTenant.JsonOptions);

        // Act
        var response = await tenant.Client.DeleteAsync($"/api/orders/{order.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await tenant.WithDbContextAsync(db => db.Orders.AnyAsync(o => o.Id == order.Id))).ShouldBeFalse();
        var refund = await tenant.WithDbContextAsync(db => db.CashSessionMovements
            .SingleAsync(m => m.CashSessionId == session.Id && m.EntryType == CashSessionEntryType.Refund));
        refund.Amount.ShouldBe(40m);
        refund.Reason.ShouldBe("Pedido Excluído");
    }

    private static async Task<Product> SeedProductAsync(ApiTenant tenant, string name, decimal price)
    {
        var product = new ProductBuilder().WithName(name).WithPrice(price)
            .WithEstablishmentId(tenant.EstablishmentId).Build();
        await tenant.SeedAsync(product);
        return product;
    }

    private static async Task<Order> SeedOrderAsync(ApiTenant tenant, (Product Product, int Quantity)[] items,
        string customer, OrderPayment[]? payments = null, OrderStatus? status = null)
    {
        var builder = new OrderBuilder()
            .WithEstablishmentId(tenant.EstablishmentId)
            .WithCustomerName(customer)
            .WithDeliveryFee(0m)
            .WithItems(items.Select(i => new OrderItemBuilder().WithProduct(i.Product)
                .WithQuantity(i.Quantity).WithEstablishmentId(tenant.EstablishmentId).Build()).ToArray());
        if (payments is not null)
            builder.WithCustomPayments(payments);
        else
            builder.WithPaymentMethod(PaymentMethod.Cash);
        if (status is not null)
            builder.WithStatus(status.Value);

        var order = builder.Build();
        await tenant.SeedAsync(order);
        return order;
    }

    private static Task SetCreatedAtAsync(ApiTenant tenant, Order order, DateTime createdAtUtc) =>
        tenant.ExecuteSqlAsync($"update orders set created_at = {createdAtUtc} where id = {order.Id}");

    private static Task<Order> LoadOrderAsync(ApiTenant tenant, Guid orderId) =>
        tenant.WithDbContextAsync(db => db.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .SingleAsync(o => o.Id == orderId));
}