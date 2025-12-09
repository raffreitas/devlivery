using Devlivery.WebApi.Features.CashRegister.Commands.CloseCashSession;
using Devlivery.WebApi.Features.CashRegister.Commands.CreateCashSession;
using Devlivery.WebApi.Features.CashRegister.Commands.CreateCashDeposit;
using Devlivery.WebApi.Features.CashRegister.Queries.GetActiveCashSession;
using Devlivery.WebApi.Features.CashRegister.Queries.GetCashSessionById;
using Devlivery.WebApi.Features.CashRegister.Queries.GetCashSessionDeposits;
using Devlivery.WebApi.Features.CashRegister.Queries.GetCashSessions;

namespace Devlivery.WebApi.Features.CashRegister;

public static class CashRegisterFeature
{
    public static IServiceCollection AddCashRegisterFeature(this IServiceCollection services)
    {
        services.AddScoped<CreateCashSessionHandler>();
        services.AddScoped<CloseCashSessionHandler>();
        services.AddScoped<CreateCashDepositHandler>();
        services.AddScoped<GetActiveCashSessionHandler>();
        services.AddScoped<GetCashSessionByIdHandler>();
        services.AddScoped<GetCashSessionDepositsHandler>();
        services.AddScoped<GetCashSessionsHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapCashRegisterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cash-sessions").WithTags("CashRegister");

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