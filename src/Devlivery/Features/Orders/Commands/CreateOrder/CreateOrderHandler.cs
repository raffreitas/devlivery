using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Entities;
using Devlivery.Features.Orders.Domain.ValueObjects;
using Devlivery.Features.Products.Domain;
using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor) : ICommandHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    public async ValueTask<Result<CreateOrderResponse>> Handle(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
        var productsDictionary = products.ToDictionary(p => p.Id, p => p);

        if (products.Count != productIds.Count)
            return Result.Fail<CreateOrderResponse>(new NotFoundError("Um ou mais produtos não foram encontrados"));

        var unavailableProducts = products.Where(p => !p.Available).ToList();
        if (unavailableProducts.Count != 0)
        {
            var productNames = string.Join(", ", unavailableProducts.Select(p => p.Name));
            return Result.Fail<CreateOrderResponse>(
                new ValidationError($"Os seguintes produtos estão indisponíveis: {productNames}"));
        }

        var customer = CustomerInfo.Create(command.CustomerName, command.CustomerPhone);
        var deliveryAddress = new DeliveryAddress(command.DeliveryAddress, command.DeliveryReference);
        var items = command.Items.Select(item => new OrderItem(
            productId: item.ProductId,
            establishmentId: tenantAccessor.Tenant.Id,
            quantity: item.Quantity,
            unitPrice: productsDictionary[item.ProductId].Price,
            notes: item.Notes)).ToList();
        var payments = command.Payments
            .Where(p => p.Amount > 0)
            .Select(p => new OrderPayment(
                establishmentId: tenantAccessor.Tenant.Id,
                paymentMethod: p.Method,
                amount: p.Amount
            )).ToList();

        var order = new Order(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: command.DeliveryFee,
            establishmentId: tenantAccessor.Tenant.Id,
            items: items,
            payments: payments,
            notes: command.Notes
        );

        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateOrderResponse(order.Id);
    }
}