using Bogus;

using Devlivery.Domain.Aggregates.Expenses;

namespace Devlivery.Tests.Common.Builders;

public sealed class ExpenseBuilder
{
    private readonly Faker _faker = new("pt_BR");

    private Guid _categoryId;
    private Guid _establishmentId;
    private decimal _amount;
    private DateOnly _dueDate;
    private string? _supplier;
    private string? _description;
    private DateOnly? _paymentDate;
    private bool _isCancelled;

    public ExpenseBuilder()
    {
        _categoryId = Guid.NewGuid();
        _establishmentId = Guid.NewGuid();
        _amount = _faker.Random.Decimal(10, 1000);
        _dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        _supplier = _faker.Company.CompanyName();
        _description = _faker.Lorem.Sentence();
    }

    public ExpenseBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public ExpenseBuilder WithEstablishmentId(Guid establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public ExpenseBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public ExpenseBuilder WithDueDate(DateOnly dueDate)
    {
        _dueDate = dueDate;
        return this;
    }

    public ExpenseBuilder WithSupplier(string? supplier)
    {
        _supplier = supplier;
        return this;
    }

    public ExpenseBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ExpenseBuilder AsPaid(DateOnly? paymentDate = null)
    {
        _paymentDate = paymentDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return this;
    }

    public ExpenseBuilder AsCancelled()
    {
        _isCancelled = true;
        return this;
    }

    public Expense Build()
    {
        var expense = new Expense(
            establishmentId: _establishmentId,
            categoryId: _categoryId,
            amount: _amount,
            dueDate: _dueDate,
            supplier: _supplier,
            description: _description,
            paymentDate: _paymentDate
        );

        if (_isCancelled)
            expense.Cancel();

        return expense;
    }
}