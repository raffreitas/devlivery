using Devlivery.Domain.Aggregates.CashRegister.Abstractions;
using Devlivery.Infrastructure.Persistence.Repositories;

namespace Devlivery.Features.CashRegister;

public static class CashRegisterFeature
{
    public static IServiceCollection AddCashRegisterFeature(this IServiceCollection services)
    {
        services.AddScoped<ICashSessionRepository, CashSessionRepository>();

        return services;
    }
}