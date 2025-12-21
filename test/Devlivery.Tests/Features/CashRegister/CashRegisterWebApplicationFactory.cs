using Devlivery.Tests.Common;

namespace Devlivery.Tests.Features.CashRegister;

public sealed class CashRegisterWebApplicationFactory : BaseWebApplicationFactory<Program>;

[CollectionDefinition("CashRegister Tests")]
public sealed class CashRegisterTestCollection : ICollectionFixture<CashRegisterWebApplicationFactory>;
