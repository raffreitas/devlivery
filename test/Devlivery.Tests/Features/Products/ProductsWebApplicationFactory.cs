using Devlivery.Tests.Common;

namespace Devlivery.Tests.Features.Products;

public sealed class ProductsWebApplicationFactory : BaseWebApplicationFactory<Program>;

[CollectionDefinition("Products Tests")]
public sealed class ProductsTestCollection : ICollectionFixture<ProductsWebApplicationFactory>;