using System.ComponentModel.DataAnnotations;

namespace Devlivery.WebApi.Shared.Identity.Tokens.Settings;

public sealed record JwtTokenSettings
{
    public const string SectionName = nameof(JwtTokenSettings);

    [Required] public required string Issuer { get; init; }
    [Required] public required string Audience { get; init; }
    [Required] public required int ExpirationInMinutes { get; init; }
    [Required] public required string SecretKey { get; init; }
};