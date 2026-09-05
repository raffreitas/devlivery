using System.ComponentModel.DataAnnotations;

namespace Devlivery.Infrastructure.Identity.LoginProtection;

public sealed class LoginProtectionOptions
{
    public const string SectionName = "LoginProtection";
    [Range(1, 100)] public int MaxFailedAccessAttempts { get; set; } = 5;
    [Range(1, 1440)] public int LockoutMinutes { get; set; } = 5;
    [Range(1, 10000)] public int PermitLimit { get; set; } = 30;
    [Range(1, 3600)] public int WindowSeconds { get; set; } = 60;
    public bool RailwayIngress { get; set; }
}
