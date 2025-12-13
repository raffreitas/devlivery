using Devlivery.Shared.Application.Errors;

namespace Devlivery.Features.Products.Shared;

public static class ProductErrors
{
    public static NotFoundError ProductNotFound =>
        new("Produto não encontrado");

    public static DomainRuleError ProductInUse =>
        new("Não é possível excluir um produto que já foi atribuido a um pedido.");
}
