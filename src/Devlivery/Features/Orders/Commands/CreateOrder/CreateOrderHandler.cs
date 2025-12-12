using Devlivery.Features.Orders.Domain;
using Devlivery.Features.Orders.Infrastructure;
using Devlivery.Features.Products.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;
using FluentResults;

namespace Devlivery.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler(
    OrderRepository orderRepository,
    ProductRepository productRepository,
    UnitOfWork unitOfWork,
    ITenantAccessor tenantAccessor)
{
    public async Task<Result<CreateOrderResponse>> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<PaymentMethod>(command.PaymentMethod, ignoreCase: true, out var paymentMethod))
            return Result.Fail("Método de pagamento inválido");

        // Buscar produtos usando Repository
        var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);
        var productsDictionary = products.ToDictionary(p => p.Id, p => p);

        if (products.Count != productIds.Count)
            return Result.Fail("Um ou mais produtos não foram encontrados");

        // Criar Order (domain logic)
        var order = new Order(
            customerName: command.CustomerName,
            customerPhone: command.CustomerPhone,
            deliveryAddress: command.DeliveryAddress,
            paymentMethod: paymentMethod,
            status: OrderStatus.Pending,
            deliveryFee: command.DeliveryFee,
            establishmentId: tenantAccessor.Tenant.Id,
            notes: command.Notes
        );
        
        foreach (var item in command.Items)
        {
            var orderItem = new OrderItem(
                productId: item.ProductId,
                establishmentId: order.EstablishmentId,
                quantity: item.Quantity,
                unitPrice: productsDictionary[item.ProductId].Price,
                notes: item.Notes);

            order.AddItem(orderItem);
        }

        // Persistir usando Repository
        await orderRepository.AddAsync(order, cancellationToken);
        
        // Disparar domain event
        order.RaiseCreatedEvent();

        // Salvar via UnitOfWork (dispara eventos automaticamente via interceptor)
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new CreateOrderResponse(order.Id);
    }
}