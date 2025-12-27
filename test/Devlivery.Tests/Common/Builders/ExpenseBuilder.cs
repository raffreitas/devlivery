using Bogus;

using Devlivery.Features.Expenses.Domain.Aggregates.Expenses;
using Devlivery.Features.Expenses.Domain.Aggregates.Expenses.Enums;

namespace Devlivery.Tests.Common.Builders;

public sealed class ExpenseBuilder
{
    private readonly Faker _faker = new();

    private Guid _categoryId;
    private Guid _establishmentId;
    private decimal _amount;
    private DateOnly _dueDate;
    private string? _supplier;
    private string? _description;
    private DateOnly? _paymentDate;
    private ExpenseStatus? _status;

    public ExpenseBuilder()
    {
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

    public ExpenseBuilder WithPaymentDate(DateOnly? paymentDate)
    {
        _paymentDate = paymentDate;
        return this;
    }

    public ExpenseBuilder WithStatus(ExpenseStatus status)
    {
        _status = status;
        return this;
    }

    public Expense Build()
    {
        if (_establishmentId == Guid.Empty)
            throw new InvalidOperationException("No establishment id has been added");

        if (_categoryId == Guid.Empty)
            _categoryId = Guid.NewGuid();

        var expense = new Expense(
            establishmentId: _establishmentId,
            categoryId: _categoryId,
            amount: _amount,
            dueDate: _dueDate,
            supplier: _supplier,
            description: _description,
            paymentDate: _paymentDate
        );

        // Ajustar status se necessário (após criação)
        if (_status == ExpenseStatus.Cancelled && expense.Status != ExpenseStatus.Cancelled)
        {
            expense.Cancel();
        }

        return expense;
    }
}

