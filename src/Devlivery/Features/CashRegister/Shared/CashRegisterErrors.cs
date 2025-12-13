using Devlivery.Shared.Application.Errors;

namespace Devlivery.Features.CashRegister.Shared;

public static class CashRegisterErrors
{
    public static DomainRuleError CashSessionAlreadyOpen =>
        new("Já existe um caixa aberto. Feche o caixa atual antes de abrir um novo.");

    public static DomainRuleError CashSessionAlreadyClosed =>
        new("O caixa já está fechado.");

    public static NotFoundError CashSessionNotFound =>
        new("Caixa não encontrado.");

    public static DomainRuleError CashSessionNotOpen =>
        new("Não é possível adicionar aporte a um caixa fechado.");
}
