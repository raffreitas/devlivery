using Devlivery.Common.Events;
using Devlivery.Common.Validation;

namespace Devlivery.Common.Handlers;

public static class HandlersRegistration
{
    /// <summary>
    /// Registers every command, query and domain event handler of the assembly by its interface.
    /// Command handlers are decorated with validation.
    /// </summary>
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<ICommandHandler<object, object>>()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationCommandHandlerDecorator<,>));

        return services;
    }
}