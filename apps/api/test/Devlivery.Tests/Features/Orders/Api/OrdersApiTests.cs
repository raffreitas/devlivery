using System.Net;
using System.Net.Http.Json;

using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Aggregates.Orders;
using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Products;
using Devlivery.Domain.Common.Enums;
using Devlivery.Features.CashRegister.Commands.CreateCashSession;
using Devlivery.Features.Dashboard.Queries.GetPaymentBreakdown;
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
        var response = await tenant.Client.GetAsync("/api/v1/orders");

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
        var response = await tenant.Client.GetAsync("/api/v1/orders?start=2026-10-14&end=2026-10-14");

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
        var response = await tenant.Client.GetAsync("/api/v1/orders?paymentMethod=Pix");

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
        var response = await tenant.Client.GetAsync($"/api/v1/orders/{order.Id}");

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
        var response = await tenant.Client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GivenNoActiveCashSession_WhenCreatingOrder_ThenReturns201WithoutConfirmingPaymentsOrOpeningCash()
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
        var response = await tenant.Client.PostAsJsonAsync("/api/v1/orders", command, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await ApiTenant.ReadDataAsync<CreateOrderResponse>(response);
        var persisted = await LoadOrderAsync(tenant, created.OrderId);
        persisted.Total.ShouldBe(85m);
        persisted.Items.ShouldHaveSingleItem().UnitPrice.ShouldBe(40m);
        persisted.Payments.ShouldHaveSingleItem().Amount.ShouldBe(100m);
        persisted.Status.ShouldBe(OrderStatus.Pending);
        persisted.Payments.ShouldAllBe(p => p.PaymentStatus == PaymentStatus.Pending);
        (await tenant.WithDbContextAsync(db => db.CashSessions.AnyAsync())).ShouldBeFalse();
        (await tenant.WithDbContextAsync(db => db.CashSessionMovements.AnyAsync())).ShouldBeFalse();
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
        var response = await tenant.Client.PutAsJsonAsync($"/api/v1/orders/{order.Id}", request, ApiTenant.JsonOptions);

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
        var response = await tenant.Client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status",
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
        var response = await tenant.Client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status",
            new { Status = OrderStatus.Preparing }, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await ApiTenant.ReadProblemAsync(response)).Detail
            .ShouldBe("Não é possível alterar o status de um pedido já entregue.");
        (await LoadOrderAsync(tenant, order.Id)).Status.ShouldBe(OrderStatus.Delivered);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenDeliveredOrderWithChange_WhenDeleting_ThenReturns422AndPreservesFinancialHistory(bool closeSession)
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var pizza = await SeedProductAsync(tenant, "Pizza", 12.34m);
        var order = await SeedOrderAsync(tenant, [(pizza, 1)], customer: "Maria Souza", payments:
        [
            new OrderPaymentBuilder().WithEstablishmentId(tenant.EstablishmentId)
                .WithPaymentMethod(PaymentMethod.Cash).WithAmount(20m).Build()
        ]);
        var session = new CashSessionBuilder().WithEstablishmentId(tenant.EstablishmentId)
            .WithAttendantId(tenant.Operator.Id).WithOpeningAmount(0m).Build();
        await tenant.SeedAsync(session);
        await tenant.Client.PatchAsJsonAsync($"/api/v1/orders/{order.Id}/status",
            new { Status = OrderStatus.Delivered }, ApiTenant.JsonOptions);

        if (closeSession)
            await tenant.Client.PatchAsJsonAsync($"/api/v1/cash-register/sessions/{session.Id}/close",
                new { ClosingAmount = 12.34m }, ApiTenant.JsonOptions);

        // Act
        var response = await tenant.Client.DeleteAsync($"/api/v1/orders/{order.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await LoadOrderAsync(tenant, order.Id)).Status.ShouldBe(OrderStatus.Delivered);
        var persisted = await tenant.WithDbContextAsync(db => db.CashSessions
            .Include(s => s.Movements).SingleAsync(s => s.Id == session.Id));
        persisted.TotalRevenue.ShouldBe(12.34m);
        persisted.ExpectedCashAmount.ShouldBe(12.34m);
        persisted.Movements.ShouldNotContain(m => m.EntryType == CashSessionEntryType.Refund);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenNoActiveCashSession_WhenOperatingOrder_ThenDeliversAndPreservesReceiptsWithoutCashMovements(bool hasClosedSession)
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var product = await SeedProductAsync(tenant, "Pizza", 10m);
        if (hasClosedSession)
        {
            await tenant.SeedAsync(new CashSessionBuilder().WithEstablishmentId(tenant.EstablishmentId)
                .WithAttendantId(tenant.Operator.Id).WithOpeningAmount(100m).AsClosed().Build());
        }
        var command = new CreateOrderCommand(
            Items: [new CreateItem(product.Id, 1, "Sem cebola")],
            CustomerName: "Cliente WhatsApp", CustomerPhone: null, DeliveryAddress: "Rua A, 1",
            Payments: [new CreatePayment(PaymentMethod.Pix, 6m), new CreatePayment(PaymentMethod.Cash, 10m)],
            DeliveryFee: 0m);

        // Act
        var response = await tenant.Client.PostAsJsonAsync("/api/v1/orders", command, ApiTenant.JsonOptions);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await ApiTenant.ReadDataAsync<CreateOrderResponse>(response);
        foreach (var status in new[] { OrderStatus.Preparing, OrderStatus.Ready, OrderStatus.Delivered })
        {
            var updated = await tenant.Client.PatchAsJsonAsync($"/api/v1/orders/{created.OrderId}/status",
                new { Status = status }, ApiTenant.JsonOptions);
            updated.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        // Assert
        var persisted = await LoadOrderAsync(tenant, created.OrderId);
        persisted.Status.ShouldBe(OrderStatus.Delivered);
        persisted.Payments.Count.ShouldBe(2);
        persisted.Payments.ShouldAllBe(p => p.PaymentStatus == PaymentStatus.Confirmed && p.ConfirmedAt != null);
        persisted.Payments.Sum(p => p.Amount).ShouldBe(16m);
        persisted.Change.ShouldBe(6m);
        (await tenant.WithDbContextAsync(db => db.CashSessions.CountAsync())).ShouldBe(hasClosedSession ? 1 : 0);
        (await tenant.WithDbContextAsync(db => db.CashSessionMovements.AnyAsync())).ShouldBeFalse();
        var breakdown = await ApiTenant.ReadDataAsync<GetPaymentBreakdownResponse>(
            await tenant.Client.GetAsync("/api/v1/dashboard/payment-breakdown"));
        breakdown.Total.ShouldBe(10m);
        breakdown.Breakdown[PaymentMethod.Pix].ShouldBe(6m);
        breakdown.Breakdown[PaymentMethod.Cash].ShouldBe(4m);

        // A later session must not absorb a sale completed outside it, even after a status retry.
        var opened = await tenant.Client.PostAsJsonAsync("/api/v1/cash-register/sessions",
            new CreateCashSessionCommand(100m, null), ApiTenant.JsonOptions);
        opened.StatusCode.ShouldBe(HttpStatusCode.Created);
        var repeated = await tenant.Client.PatchAsJsonAsync($"/api/v1/orders/{created.OrderId}/status",
            new { Status = OrderStatus.Delivered }, ApiTenant.JsonOptions);
        repeated.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var afterRetry = await LoadOrderAsync(tenant, created.OrderId);
        afterRetry.Payments.Select(p => p.ConfirmedAt).Order().ShouldBe(persisted.Payments.Select(p => p.ConfirmedAt).Order());
        (await tenant.WithDbContextAsync(db => db.CashSessionMovements.AnyAsync())).ShouldBeFalse();
    }

    [Theory]
    [InlineData(PaymentMethod.Pix)]
    [InlineData(PaymentMethod.CreditCard)]
    [InlineData(PaymentMethod.DebitCard)]
    public async Task GivenElectronicPaymentAboveTotal_WhenCreating_ThenReturns422WithoutSaving(PaymentMethod method)
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var product = await SeedProductAsync(tenant, "Pizza", 10m);
        var command = new CreateOrderCommand(
            Items: [new CreateItem(product.Id, 1, null)], CustomerName: "Cliente",
            CustomerPhone: null, DeliveryAddress: "Rua A",
            Payments: [new CreatePayment(PaymentMethod.Cash, 1m), new CreatePayment(method, 20m)], DeliveryFee: 0m);

        // Act
        var response = await tenant.Client.PostAsJsonAsync("/api/v1/orders", command, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await tenant.WithDbContextAsync(db => db.Orders.AnyAsync())).ShouldBeFalse();
    }

    [Fact]
    public async Task GivenElectronicPaymentAboveTotal_WhenUpdating_ThenReturns422AndPreservesOriginalPayments()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var product = await SeedProductAsync(tenant, "Pizza", 10m);
        var order = await SeedOrderAsync(tenant, [(product, 1)], "Cliente");
        var request = new
        {
            Items = new[] { new { ProductId = product.Id, Quantity = 1, Notes = (string?)null } },
            CustomerName = "Cliente", CustomerPhone = (string?)null, DeliveryAddress = "Rua B", DeliveryFee = 0m,
            Payments = new[] { new { Id = order.Payments.Single().Id, Method = PaymentMethod.Pix, Amount = 20m } }
        };

        // Act
        var response = await tenant.Client.PutAsJsonAsync($"/api/v1/orders/{order.Id}", request, ApiTenant.JsonOptions);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var persisted = await LoadOrderAsync(tenant, order.Id);
        persisted.DeliveryAddress.FullAddress.ShouldBe(order.DeliveryAddress.FullAddress);
        persisted.Payments.ShouldHaveSingleItem().Amount.ShouldBe(10m);
        persisted.Payments.Single().PaymentMethod.ShouldBe(PaymentMethod.Cash);
    }

    [Fact]
    public async Task GivenRepeatedProductWithDifferentNotes_WhenCreatingAndUpdating_ThenPreservesBothLines()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var product = await SeedProductAsync(tenant, "Pizza", 10m);
        var command = new CreateOrderCommand(
            Items: [new CreateItem(product.Id, 1, "Sem cebola"), new CreateItem(product.Id, 1, "Com cebola")],
            CustomerName: "Cliente", CustomerPhone: null, DeliveryAddress: "Rua A",
            Payments: [new CreatePayment(PaymentMethod.Cash, 20m)], DeliveryFee: 0m);

        // Act
        var createdResponse = await tenant.Client.PostAsJsonAsync("/api/v1/orders", command, ApiTenant.JsonOptions);
        var created = await ApiTenant.ReadDataAsync<CreateOrderResponse>(createdResponse);
        var persisted = await LoadOrderAsync(tenant, created.OrderId);
        var updateResponse = await tenant.Client.PutAsJsonAsync($"/api/v1/orders/{created.OrderId}", new
        {
            command.Items, command.CustomerName, command.CustomerPhone, DeliveryAddress = "Rua B", command.DeliveryFee,
            Payments = new[] { new { Id = persisted.Payments.Single().Id, Method = PaymentMethod.Cash, Amount = 20m } }
        }, ApiTenant.JsonOptions);
        var detail = await ApiTenant.ReadDataAsync<GetOrderByIdResponse>(
            await tenant.Client.GetAsync($"/api/v1/orders/{created.OrderId}"));

        // Assert
        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        detail.Items.Select(i => i.Notes).ShouldBe(["Sem cebola", "Com cebola"], ignoreOrder: true);
        detail.Items.ShouldAllBe(i => i.Quantity == 1 && i.UnitPrice == 10m);
        detail.Total.ShouldBe(20m);
    }

    [Fact]
    public async Task GivenCatalogPriceChanged_WhenListingAndGettingOrder_ThenReturnsRecordedUnitPrice()
    {
        // Arrange
        var tenant = await api.CreateTenantAsync();
        var product = await SeedProductAsync(tenant, "Pizza", 9.90m);
        var order = await SeedOrderAsync(tenant, [(product, 3)], customer: "Cliente");
        await tenant.WithDbContextAsync(async db =>
        {
            var storedProduct = await db.Products.SingleAsync(p => p.Id == product.Id);
            storedProduct.Update(price: 20m);
            await db.SaveChangesAsync();
        });

        // Act
        var listResponse = await tenant.Client.GetAsync("/api/v1/orders");
        var detailResponse = await tenant.Client.GetAsync($"/api/v1/orders/{order.Id}");

        // Assert
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        detailResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var listed = (await ApiTenant.ReadDataAsync<List<GetAllOrdersResponse>>(listResponse)).ShouldHaveSingleItem();
        var detail = await ApiTenant.ReadDataAsync<GetOrderByIdResponse>(detailResponse);
        listed.Items.ShouldHaveSingleItem().UnitPrice.ShouldBe(9.90m);
        detail.Items.ShouldHaveSingleItem().UnitPrice.ShouldBe(9.90m);
        listed.Items[0].Product.Price.ShouldBe(20m);
        detail.Items[0].Product.Price.ShouldBe(20m);
        listed.Total.ShouldBe(29.70m);
        detail.Total.ShouldBe(29.70m);
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
