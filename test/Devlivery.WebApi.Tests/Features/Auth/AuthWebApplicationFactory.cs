using Devlivery.WebApi.Tests.Common;

namespace Devlivery.WebApi.Tests.Features.Auth;

public sealed class AuthWebApplicationFactory : BaseWebApplicationFactory<Program>;

[CollectionDefinition("Auth Tests")]
public sealed class AuthTestCollection : ICollectionFixture<AuthWebApplicationFactory>;