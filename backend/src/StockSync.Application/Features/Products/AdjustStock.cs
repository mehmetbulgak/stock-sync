using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Domain.Common;
using StockSync.Domain.Products;

namespace StockSync.Application.Features.Products;

public static class AdjustStock
{
    public sealed record Command(int Quantity, string Reason);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Quantity).NotEqual(0).InclusiveBetween(-Product.MaxStock, Product.MaxStock);
            RuleFor(c => c.Reason).NotEmpty().MaximumLength(StockMovement.NoteMaxLength);
        }
    }

    public sealed class Handler(IAppDbContext db, TimeProvider clock)
    {
        public async Task<Result<ProductDto>> HandleAsync(Guid id, Command command, CancellationToken ct)
        {
            var product = await db.Products.SingleOrDefaultAsync(p => p.Id == id, ct);
            if (product is null)
            {
                return ProductErrors.NotFound(id);
            }

            var result = product.AdjustStock(command.Quantity, command.Reason, clock.GetUtcNow());
            if (result.IsFailure)
            {
                return result.Error!;
            }

            await db.SaveChangesAsync(ct);
            return ProductDto.From(product);
        }
    }
}
