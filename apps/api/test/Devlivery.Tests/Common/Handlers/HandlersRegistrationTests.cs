using Devlivery.Common.Events;
using Devlivery.Common.Handlers;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Features.CashRegister.Events;
using Devlivery.Features.Orders.Commands.CreateOrder;
using Devlivery.Features.Products.Queries.GetAllProducts;

using FluentResults;

using Microsoft.Extensions.DependencyInjection;

using Shouldly;

namespace Devlivery.Tests.Common.Handlers;

[Trait("Category", "Unit Tests")]
public sealed class HandlersRegistrationTests
{
    private static readonly Type[] HandlerInterfaces =
    [
        typeof(ICommandHandler<,>),
        typeof(IQueryHandler<,>),
        typeof(IDomainEventHandler<>)
    ];

    [Fact]
    public void AddHandlers_Should_Register_Every_Handler_By_Its_Interface_As_Scoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var handlerImplementations = typeof(ICommandHandler<,>).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && HandlerInterfaces.Contains(i.GetGenericTypeDefinition()))
                .Select(i => (Service: i, Implementation: type)))
            .ToList();

        // Act
        services.AddHandlers();

        // Assert
        handlerImplementations.ShouldNotBeEmpty();
        foreach (var (service, implementation) in handlerImplementations)
        {
            services.ShouldContain(
                d => d.ServiceType == service
                     && d.ImplementationType == implementation
                     && d.Lifetime == ServiceLifetime.Scoped,
                $"{implementation.Name} should be registered as {service.Name}");
        }
    }

    [Theory]
    [InlineData(typeof(ICommandHandler<CreateOrderCommand, Result<CreateOrderResponse>>), typeof(CreateOrderHandler))]
    [InlineData(typeof(IQueryHandler<GetAllProductsQuery, Result<List<GetAllProductsResponse>>>), typeof(GetAllProductsHandler))]
    [InlineData(typeof(IDomainEventHandler<OrderPaymentConfirmedEvent>), typeof(OrderPaymentConfirmedEventHandler))]
    public void AddHandlers_Should_Register_Known_Handlers(Type service, Type implementation)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHandlers();

        // Assert
        services.ShouldContain(d => d.ServiceType == service && d.ImplementationType == implementation);
    }
}