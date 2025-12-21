using Devlivery.Tests.Common;

namespace Devlivery.Tests.Features.Orders;

public sealed class OrdersWebApplicationFactory : BaseWebApplicationFactory<Program>;

[CollectionDefinition("Orders Tests")]
public sealed class OrdersTestCollection : ICollectionFixture<OrdersWebApplicationFactory>;