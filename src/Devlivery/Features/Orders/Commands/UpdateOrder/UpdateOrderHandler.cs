using Devlivery.Features.Orders.Domain;
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
                new DomainRuleError("Pedido não pode ser atualizado pois está cancelado ou já foi entregue"));

        var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);

        if (products.Count != productIds.Count)
            return Result.Fail(new NotFoundError("Um ou mais produtos não foram encontrados"));

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
            paymentMethod: command.PaymentMethod,
            deliveryFee: command.DeliveryFee,
            notes: command.Notes
        );

        await orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}