using Devlivery.Features.CashRegister.Commands.CloseCashSession;
using Devlivery.Features.CashRegister.Commands.CreateCashDeposit;
using Devlivery.Features.CashRegister.Commands.CreateCashSession;
using Devlivery.Features.CashRegister.Events;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.CashRegister.Queries.GetActiveCashSession;
using Devlivery.Features.CashRegister.Queries.GetCashSessionById;
using Devlivery.Features.CashRegister.Queries.GetCashSessionDeposits;
using Devlivery.Features.CashRegister.Queries.GetCashSessions;

namespace Devlivery.Features.CashRegister;

public static class CashRegisterFeature
{
    public static IServiceCollection AddCashRegisterFeature(this IServiceCollection services)
    {
        // Register Repository
        services.AddScoped<ICashSessionRepository, CashSessionRepository>();

        // Register Domain Event Handlers
        services.AddScoped<OrderPaymentConfirmedEventHandler>();
        services.AddScoped<OrderDeletedEventHandler>();
        services.AddScoped<OrderStatusChangedEventHandler>();
        services.AddScoped<OrderChangeCalculatedEventHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapCashRegisterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cash-register").WithTags("CashRegister");

        CreateCashSessionEndpoint.MapEndpoint(group);
        CloseCashSessionEndpoint.MapEndpoint(group);
        CreateCashDepositEndpoint.MapEndpoint(group);
        GetActiveCashSessionEndpoint.MapEndpoint(group);
        GetCashSessionByIdEndpoint.MapEndpoint(group);
        GetCashSessionDepositsEndpoint.MapEndpoint(group);
        GetCashSessionsEndpoint.MapEndpoint(group);

        return app;
    }
}