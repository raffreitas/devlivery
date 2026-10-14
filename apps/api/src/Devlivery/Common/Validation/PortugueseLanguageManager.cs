using System.Globalization;
using System.Runtime.CompilerServices;

using FluentValidation;
using FluentValidation.Resources;

namespace Devlivery.Common.Validation;

/// <summary>
/// Standard pt-BR messages for the FluentValidation rules used by the API, so validators
/// only declare rules and field names (<c>.WithName(...)</c>) instead of repeating messages.
/// Rules without an entry here fall back to FluentValidation's built-in pt-BR messages.
/// </summary>
public sealed class PortugueseLanguageManager : LanguageManager
{
    private const string BrazilianPortuguese = "pt-BR";

    public PortugueseLanguageManager()
    {
        Culture = new CultureInfo(BrazilianPortuguese);

        AddTranslation(BrazilianPortuguese, "NotEmptyValidator", "O campo '{PropertyName}' é obrigatório.");
        AddTranslation(BrazilianPortuguese, "NotNullValidator", "O campo '{PropertyName}' é obrigatório.");
        AddTranslation(BrazilianPortuguese, "MaximumLengthValidator", "O campo '{PropertyName}' deve ter no máximo {MaxLength} caracteres.");
        AddTranslation(BrazilianPortuguese, "MinimumLengthValidator", "O campo '{PropertyName}' deve ter no mínimo {MinLength} caracteres.");
        AddTranslation(BrazilianPortuguese, "GreaterThanValidator", "O campo '{PropertyName}' deve ser maior que {ComparisonValue}.");
        AddTranslation(BrazilianPortuguese, "GreaterThanOrEqualValidator", "O campo '{PropertyName}' deve ser maior ou igual a {ComparisonValue}.");
        AddTranslation(BrazilianPortuguese, "EmailValidator", "O campo '{PropertyName}' deve ser um e-mail válido.");
        AddTranslation(BrazilianPortuguese, "EnumValidator", "O campo '{PropertyName}' possui um valor inválido.");
    }

    // Validation messages are part of the API contract, so they are configured as soon as the
    // assembly loads: the API and validators created directly in tests share the same messages.
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Configure()
    {
        ValidatorOptions.Global.LanguageManager = new PortugueseLanguageManager();
    }
}