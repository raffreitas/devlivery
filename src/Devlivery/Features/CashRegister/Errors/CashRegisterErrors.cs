using Devlivery.Shared.SeedWork.Errors;

namespace Devlivery.Features.CashRegister.Errors;

public static class CashRegisterErrors
{
    public static BusinessRuleError CashSessionAlreadyOpen =>
        new("Já existe um caixa aberto. Feche o caixa atual antes de abrir um novo.");

    public static BusinessRuleError CashSessionAlreadyClosed =>
        new("O caixa já está fechado.");

    public static NotFoundError CashSessionNotFound =>
        new("Caixa não encontrado.");
}