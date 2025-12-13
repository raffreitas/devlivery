using Devlivery.Shared.Application.Errors;

namespace Devlivery.Features.Orders.Shared;

public static class OrderErrors
{
    public static NotFoundError OrderNotFound =>
        new("Pedido não encontrado");

    public static NotFoundError ProductNotFound =>
        new("Um ou mais produtos não foram encontrados");

    public static DomainRuleError InvalidPaymentMethod =>
        new("Método de pagamento inválido");

    public static DomainRuleError InvalidOrderStatus =>
        new("Status inválido");

    public static DomainRuleError OrderCannotBeUpdated =>
        new("Pedido não pode ser atualizado pois está cancelado ou já foi entregue");
}
