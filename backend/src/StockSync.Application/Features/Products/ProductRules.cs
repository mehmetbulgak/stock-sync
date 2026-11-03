using FluentValidation;
using StockSync.Domain.Products;

namespace StockSync.Application.Features.Products;

public static class ProductRules
{
    private const string SkuPattern = "^[A-Za-z0-9._-]+$";

    public static IRuleBuilderOptions<T, string> ValidSku<T>(this IRuleBuilder<T, string> rule) => rule
        .NotEmpty()
        .MaximumLength(Product.SkuMaxLength)
        .Matches(SkuPattern).WithMessage("SKU may only contain letters, digits, '.', '_' and '-'.");

    public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> rule) => rule
        .NotEmpty()
        .MaximumLength(Product.NameMaxLength);

    public static IRuleBuilderOptions<T, string?> ValidDescription<T>(this IRuleBuilder<T, string?> rule) => rule
        .MaximumLength(Product.DescriptionMaxLength);

    public static IRuleBuilderOptions<T, decimal> ValidPrice<T>(this IRuleBuilder<T, decimal> rule) => rule
        .InclusiveBetween(0, Product.MaxPrice)
        .PrecisionScale(12, 2, ignoreTrailingZeros: true);

    public static IRuleBuilderOptions<T, int> ValidStockLevel<T>(this IRuleBuilder<T, int> rule) => rule
        .InclusiveBetween(0, Product.MaxStock);
}
