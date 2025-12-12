using Devlivery.Shared.Infrastructure.Persistence.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductHandler(ApplicationDbContext dbContext)
{
    public async Task<Result> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

        if (product is null)
            return Result.Fail("Produto não encontrado");

        product.Update(
            name: command.Name,
            description: command.Description,
            price: command.Price,
            category: command.Category
        );

        if (command.Available)
            product.SetAsAvailable();
        else
            product.SetAsUnavailable();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}