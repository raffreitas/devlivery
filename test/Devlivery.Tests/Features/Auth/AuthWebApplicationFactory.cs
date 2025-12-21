using Devlivery.Tests.Common;

namespace Devlivery.Tests.Features.Auth;

public sealed class AuthWebApplicationFactory : BaseWebApplicationFactory<Program>;

[CollectionDefinition("Auth Tests")]
public sealed class AuthTestCollection : ICollectionFixture<AuthWebApplicationFactory>;