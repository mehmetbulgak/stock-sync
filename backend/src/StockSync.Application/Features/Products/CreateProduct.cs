using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Application.Common;
using StockSync.Domain.Common;
using StockSync.Domain.Products;

namespace StockSync.Application.Features.Products;

public static class CreateProduct
{
    public sealed record Command(string Sku, string Name, string? Description, decimal Price, int InitialStock, int LowStockThreshold);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Sku).ValidSku();
            RuleFor(c => c.Name).ValidName();
            RuleFor(c => c.Description).ValidDescription();
            RuleFor(c => c.Price).ValidPrice();
            RuleFor(c => c.InitialStock).ValidStockLevel();
            RuleFor(c => c.LowStockThreshold).ValidStockLevel();
        }
    }

    public sealed class Handler(IAppDbContext db, ICurrentUser user, TimeProvider clock)
    {
        public async Task<Result<ProductDto>> HandleAsync(Command command, CancellationToken ct)
        {
            var sku = Product.NormalizeSku(command.Sku);
            if (await db.Products.AnyAsync(p => p.Sku == sku, ct))
            {
                return ProductErrors.DuplicateSku(sku);
            }

            var product = Product.Create(
                user.GetRequiredTenantId(),
                sku,
                command.Name,
                command.Description,
                command.Price,
                command.InitialStock,
                command.LowStockThreshold,
                clock.GetUtcNow());

            if (product.IsFailure)
            {
                return product.Error!;
            }

            db.Products.Add(product.Value);
            await db.SaveChangesAsync(ct);
            return ProductDto.From(product.Value);
        }
    }
}
