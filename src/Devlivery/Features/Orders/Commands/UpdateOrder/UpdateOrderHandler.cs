using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Infrastructure;
using Devlivery.Features.Orders.Shared;
using Devlivery.Features.Products.Infrastructure;
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
        if (!command.IsValid(out var errors))
        {
            return Result.Fail(errors);
        }

        if (!Enum.TryParse<PaymentMethod>(command.PaymentMethod, ignoreCase: true, out var paymentMethod))
            return Result.Fail(OrderErrors.InvalidPaymentMethod);

        var order = await orderRepository.GetByIdAsync(command.Id, cancellationToken);

        if (order is null)
            return Result.Fail(OrderErrors.OrderNotFound);

        if (order.Status is OrderStatus.Canceled or OrderStatus.Delivered)
            return Result.Fail(OrderErrors.OrderCannotBeUpdated);

        var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);

        if (products.Count != productIds.Count)
            return Result.Fail(OrderErrors.ProductNotFound);

        var productsDictionary = products.ToDictionary(p => p.Id, p => p);

        var newItens = command.Items.Select(item => new OrderItem(
            productId: item.ProductId,
            establishmentId: order.EstablishmentId,
            quantity: item.Quantity,
            unitPrice: productsDictionary[item.ProductId].Price,
            notes: item.Notes));

        order.ReplaceItems(newItens);

        order.UpdateDetails(
            customerName: command.CustomerName,
            customerPhone: command.CustomerPhone,
            deliveryAddress: command.DeliveryAddress,
            paymentMethod: paymentMethod,
            deliveryFee: command.DeliveryFee,
            notes: command.Notes
        );

        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}