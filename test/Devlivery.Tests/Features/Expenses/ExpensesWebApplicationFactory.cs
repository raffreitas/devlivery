using Devlivery.Tests.Common;

namespace Devlivery.Tests.Features.Expenses;

public sealed class ExpensesWebApplicationFactory : BaseWebApplicationFactory<Program>;

[CollectionDefinition("Expenses Tests")]
public sealed class ExpensesTestCollection : ICollectionFixture<ExpensesWebApplicationFactory>;

