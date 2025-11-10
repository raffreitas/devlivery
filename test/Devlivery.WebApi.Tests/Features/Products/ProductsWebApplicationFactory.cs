using Devlivery.WebApi.Tests.Common;

namespace Devlivery.WebApi.Tests.Features.Products;

public sealed class ProductsWebApplicationFactory : BaseWebApplicationFactory<Program>;

[CollectionDefinition("Products Tests")]
public sealed class ProductsTestCollection : ICollectionFixture<ProductsWebApplicationFactory>;