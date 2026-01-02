using Bogus;

using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Domain.Entities;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Tenancy;
using Devlivery.Tests.Common.Builders;

using NSubstitute;

namespace Devlivery.Tests.Features.CashRegister;

/// <summary>
/// Fixture para testes de unidade da feature CashRegister.
/// Fornece factory methods para criar mocks das dependências utilizando NSubstitute.
/// </summary>
public sealed class CashRegisterUnitTestFixture : IDisposable
{
    public Faker Faker { get; } = new("pt_BR");

    private readonly Guid _defaultTenantId = Guid.NewGuid();
    private readonly Guid _defaultAttendantId = Guid.NewGuid();

    /// <summary>
    /// Cria um mock de ITenantAccessor com um tenant padrão.
    /// </summary>
    public ITenantAccessor CreateTenantAccessorMock(Guid? tenantId = null)
    {
        var mock = Substitute.For<ITenantAccessor>();
        var tenant = new Tenant(tenantId ?? _defaultTenantId);
        mock.Tenant.Returns(tenant);
        return mock;
    }

    /// <summary>
    /// Cria um mock de ICashSessionRepository.
    /// </summary>
    public ICashSessionRepository CreateCashSessionRepositoryMock()
    {
        return Substitute.For<ICashSessionRepository>();
    }

    /// <summary>
    /// Cria um mock de IUnitOfWork.
    /// </summary>
    public IUnitOfWork CreateUnitOfWorkMock()
    {
        return Substitute.For<IUnitOfWork>();
    }

    /// <summary>
    /// Cria uma instância de CashSession para uso em testes.
    /// Usa o CashSessionBuilder com valores padrão sensatos.
    /// </summary>
    public CashSession CreateCashSession(
        Guid? establishmentId = null,
        Guid? attendantId = null,
        string? attendantName = null,
        decimal? openingAmount = null,
        string? notes = null)
    {
        var builder = new CashSessionBuilder();

        builder.WithEstablishmentId(establishmentId ?? _defaultTenantId);
        builder.WithAttendantId(attendantId ?? _defaultAttendantId);

        if (!string.IsNullOrEmpty(attendantName))
            builder.WithAttendantName(attendantName);

        if (openingAmount.HasValue)
            builder.WithOpeningAmount(openingAmount.Value);

        if (!string.IsNullOrEmpty(notes))
            builder.WithNotes(notes);

        return builder.Build();
    }

    /// <summary>
    /// Cria uma instância de CashDeposit para uso em testes.
    /// Usa o CashDepositBuilder com valores padrão sensatos.
    /// </summary>
    public CashDeposit CreateCashDeposit(
        Guid? cashSessionId = null,
        Guid? establishmentId = null,
        Guid? attendantId = null,
        string? attendantName = null,
        decimal? amount = null,
        string? notes = null)
    {
        var builder = new CashDepositBuilder();

        if (cashSessionId.HasValue)
            builder.WithCashSessionId(cashSessionId.Value);

        builder.WithEstablishmentId(establishmentId ?? _defaultTenantId);
        builder.WithAttendantId(attendantId ?? _defaultAttendantId);

        if (!string.IsNullOrEmpty(attendantName))
            builder.WithAttendantName(attendantName);

        if (amount.HasValue)
            builder.WithAmount(amount.Value);

        if (!string.IsNullOrEmpty(notes))
            builder.WithNotes(notes);

        return builder.Build();
    }

    public void Dispose()
    {
        // Cleanup se necessário
    }
}

[CollectionDefinition("CashRegister Unit Tests")]
public sealed class CashRegisterUnitTestCollection : ICollectionFixture<CashRegisterUnitTestFixture>;