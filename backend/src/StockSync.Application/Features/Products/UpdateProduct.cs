using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Domain.Common;
using StockSync.Domain.Products;

namespace StockSync.Application.Features.Products;

public static class UpdateProduct
{
    public sealed record Command(string Name, string? Description, decimal Price, int LowStockThreshold, bool IsActive);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).ValidName();
            RuleFor(c => c.Description).ValidDescription();
            RuleFor(c => c.Price).ValidPrice();
            RuleFor(c => c.LowStockThreshold).ValidStockLevel();
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

            var result = product.Update(
                command.Name,
                command.Description,
                command.Price,
                command.LowStockThreshold,
                command.IsActive,
                clock.GetUtcNow());

            if (result.IsFailure)
            {
                return result.Error!;
            }

            await db.SaveChangesAsync(ct);
            return ProductDto.From(product);
        }
    }
}
