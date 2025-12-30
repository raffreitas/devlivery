using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Domain.Entities;
using Devlivery.Features.Orders.Domain.Enums;
using Devlivery.Features.Orders.Domain.ValueObjects;
using Devlivery.Features.Products.Domain;
using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence;

using FluentResults;

using Mediator;

namespace Devlivery.Features.Orders.Commands.UpdateOrder;

public sealed class UpdateOrderHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateOrderCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(command.Id, cancellationToken);

        if (order is null)
            return Result.Fail(new NotFoundError("Pedido não encontrado"));

        if (order.Status is OrderStatus.Canceled or OrderStatus.Delivered)
            return Result.Fail(
                new ValidationError("Pedido não pode ser atualizado pois está cancelado ou já foi entregue"));

        var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);

        if (products.Count != productIds.Count)
            return Result.Fail(new NotFoundError("Um ou mais produtos não foram encontrados"));

        // Verify all products are available
        var unavailableProducts = products.Where(p => !p.Available).ToList();
        if (unavailableProducts.Count != 0)
        {
            var productNames = string.Join(", ", unavailableProducts.Select(p => p.Name));
            return Result.Fail(
                new ValidationError($"Os seguintes produtos estão indisponíveis: {productNames}"));
        }

        var productsDictionary = products.ToDictionary(p => p.Id, p => p);

        if (order.PaymentMethod != command.PaymentMethod)
            order.UpdatePaymentMethod(command.PaymentMethod);

        var newItens = command.Items.Select(item => new OrderItem(
            productId: item.ProductId,
            establishmentId: order.EstablishmentId,
            quantity: item.Quantity,
            unitPrice: productsDictionary[item.ProductId].Price,
            notes: item.Notes)).ToList();

        // Create Value Objects
        var customer = CustomerInfo.Create(command.CustomerName, command.CustomerPhone);
        var deliveryAddress = new DeliveryAddress(command.DeliveryAddress, command.DeliveryReference);

        order.UpdateDetails(
            customer: customer,
            deliveryAddress: deliveryAddress,
            deliveryFee: command.DeliveryFee,
            items: newItens,
            notes: command.Notes
        );

        await orderRepository.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}