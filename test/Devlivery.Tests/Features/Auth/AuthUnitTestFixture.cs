using Bogus;

using Devlivery.Features.Users.Domain;
using Devlivery.Infrastructure.Identity.Abstractions;
using Devlivery.Infrastructure.Persistence.Context;
using Devlivery.Infrastructure.Tenancy;
using Devlivery.Tests.Common.Builders;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

namespace Devlivery.Tests.Features.Auth;

public sealed class AuthUnitTestFixture
{
    public Faker Faker { get; } = new("pt_BR");

    private readonly Guid _defaultTenantId = Guid.NewGuid();

    public ITenantAccessor CreateTenantAccessorMock(Guid? tenantId = null)
    {
        var mock = Substitute.For<ITenantAccessor>();
        var tenant = new Tenant(tenantId ?? _defaultTenantId);
        mock.Tenant.Returns(tenant);
        return mock;
    }

    public IIdentityService CreateIdentityServiceMock()
    {
        return Substitute.For<IIdentityService>();
    }

    public ITokenService CreateTokenServiceMock()
    {
        return Substitute.For<ITokenService>();
    }

    public ApplicationDbContext CreateDbContextMock(Guid? tenantId = null)
    {
        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(dbContextOptions, CreateTenantAccessorMock(tenantId));
    }

    public User CreateUser(
        string? name = null,
        string? email = null,
        Guid? establishmentId = null)
    {
        return new UserBuilder()
            .WithName(name ?? Faker.Name.FullName())
            .WithEmail(email ?? Faker.Internet.Email())
            .WithEstablishmentId(establishmentId ?? _defaultTenantId)
            .Build();
    }
}