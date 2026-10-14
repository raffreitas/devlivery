using Devlivery.Common.Errors;
using Devlivery.Common.Events;
using Devlivery.Common.Handlers;
using Devlivery.Common.Validation;
using Devlivery.Domain.Aggregates.Orders.Abstractions;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Aggregates.Products.Abstractions;
using Devlivery.Features.CashRegister.Events;
using Devlivery.Features.Orders.Commands.CreateOrder;
using Devlivery.Features.Products.Queries.GetAllProducts;
using Devlivery.Infrastructure.Persistence;
using Devlivery.Infrastructure.Tenancy;

using FluentResults;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

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
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
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
                     && ImplementationTypeOf(d) == implementation
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
        services.ShouldContain(d => d.ServiceType == service && ImplementationTypeOf(d) == implementation);
    }

    [Fact]
    public async Task AddHandlers_Should_Validate_Commands_Before_Reaching_The_Handler()
    {
        // Arrange
        var productRepository = Substitute.For<IProductRepository>();
        var services = new ServiceCollection();
        services.AddHandlers();
        services.AddValidatorsFromAssembly(typeof(ICommandHandler<,>).Assembly);
        services.AddScoped(_ => Substitute.For<IOrderRepository>());
        services.AddScoped(_ => productRepository);
        services.AddScoped(_ => Substitute.For<IUnitOfWork>());
        services.AddScoped(_ => Substitute.For<ITenantAccessor>());

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var handler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateOrderCommand, Result<CreateOrderResponse>>>();

        var invalidCommand = new CreateOrderCommand(
            Items: [],
            CustomerName: "",
            CustomerPhone: null,
            DeliveryAddress: "Rua A, 123",
            Payments: []);

        // Act
        var result = await handler.HandleAsync(invalidCommand, CancellationToken.None);

        // Assert
        handler.ShouldBeOfType<ValidationCommandHandlerDecorator<CreateOrderCommand, Result<CreateOrderResponse>>>();
        result.IsFailed.ShouldBeTrue();
        result.Errors.Single().ShouldBeOfType<ValidationError>()
            .Errors.Select(e => e.Field).ShouldBe(["Items", "CustomerName", "Payments"], ignoreOrder: true);
        await productRepository.DidNotReceiveWithAnyArgs().GetByIdsAsync(default!, default);
    }

    private static Type? ImplementationTypeOf(ServiceDescriptor descriptor) =>
        descriptor.IsKeyedService ? descriptor.KeyedImplementationType : descriptor.ImplementationType;
}