using FluentValidation;
using StockSync.Application.Abstractions;
using StockSync.Domain.Common;
using StockSync.Domain.Orders;

namespace StockSync.Application.Features.Orders;

public static class CancelOrder
{
    public sealed record Command(string? Reason);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(c => c.Reason).MaximumLength(Order.CancellationReasonMaxLength);
    }

    public sealed class Handler(IAppDbContext db, TimeProvider clock)
    {
        public Task<Result<OrderDto>> HandleAsync(Guid id, Command command, CancellationToken ct) =>
            OrderTransition.RunAsync(db, id, (order, products) => order.Cancel(products, command.Reason, clock.GetUtcNow()), ct);
    }
}
