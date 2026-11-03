using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockSync.Application.Abstractions;
using StockSync.Domain.Common;
using StockSync.Domain.Orders;
using StockSync.Domain.Products;

namespace StockSync.Application.Features.Orders;

public static class CreateOrder
{
    private const string OrderNumberPattern = "^[A-Za-z0-9._-]+$";

    public sealed record Item(Guid ProductId, int Quantity, decimal? UnitPrice = null);

    public sealed record Command(Guid ChannelId, string? Number, string CustomerName, IReadOnlyList<Item> Items);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.ChannelId).NotEmpty();
            RuleFor(c => c.Number)
                .MaximumLength(Order.NumberMaxLength)
                .Matches(OrderNumberPattern).WithMessage("Order number may only contain letters, digits, '.', '_' and '-'.")
                .When(c => !string.IsNullOrWhiteSpace(c.Number));
            RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(Order.CustomerNameMaxLength);
            RuleFor(c => c.Items).Cascade(CascadeMode.Stop).NotEmpty().Must(i => i.Count <= Order.MaxLines)
                .WithMessage($"An order can have at most {Order.MaxLines} lines.");
            RuleForEach(c => c.Items).NotNull().ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty();
                item.RuleFor(i => i.Quantity).InclusiveBetween(1, Order.MaxQuantityPerLine);
                item.RuleFor(i => i.UnitPrice)
                    .InclusiveBetween(0, Product.MaxPrice)
                    .PrecisionScale(12, 2, ignoreTrailingZeros: true)
                    .When(i => i.UnitPrice is not null);
            });
        }
    }

    public sealed class Handler(IAppDbContext db, TimeProvider clock)
    {
        public async Task<Result<OrderDto>> HandleAsync(Command command, CancellationToken ct)
        {
            var channel = await db.SalesChannels.SingleOrDefaultAsync(c => c.Id == command.ChannelId, ct);
            if (channel is null)
            {
                return Error.NotFound("Channel.NotFound", "Sales channel not found.");
            }

            var now = clock.GetUtcNow();
            var number = string.IsNullOrWhiteSpace(command.Number)
                ? OrderNumberGenerator.Generate(channel.Type, now)
                : command.Number.Trim();

            if (await db.Orders.AnyAsync(o => o.ChannelId == channel.Id && o.Number == number, ct))
            {
                return OrderErrors.Duplicate(number);
            }

            var productIds = command.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);
            if (products.Count != productIds.Count)
            {
                return OrderErrors.ProductMissing;
            }

            var items = command.Items
                .Select(i => new OrderItem(products[i.ProductId], i.Quantity, i.UnitPrice))
                .ToList();

            var order = Order.Place(channel, number, command.CustomerName, items, now);
            if (order.IsFailure)
            {
                return order.Error!;
            }

            db.Orders.Add(order.Value);
            await db.SaveChangesAsync(ct);
            return OrderDto.From(order.Value);
        }
    }
}
