using Devlivery.Shared.Extensions;

using FluentResults;

using FluentValidation;

using Mediator;

namespace Devlivery.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand<Result<DeleteProductResponse>>
{
    public bool IsValid(out string[] errors)
    {
        var result = new DeleteProductCommandValidator().Validate(this);
        errors = result.GetErrors();
        return result.IsValid;
    }
};

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O campo '{PropertyName}' é obrigatório.");
    }
}