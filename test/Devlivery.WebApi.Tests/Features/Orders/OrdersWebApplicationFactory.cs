using Devlivery.WebApi.Tests.Common;

namespace Devlivery.WebApi.Tests.Features.Orders;

public sealed class OrdersWebApplicationFactory : BaseWebApplicationFactory<Program>;

[CollectionDefinition("Orders Tests")]
public sealed class OrdersTestCollection : ICollectionFixture<OrdersWebApplicationFactory>;