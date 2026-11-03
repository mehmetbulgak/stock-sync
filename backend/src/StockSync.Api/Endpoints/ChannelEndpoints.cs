using StockSync.Application.Features.Channels;

namespace StockSync.Api.Endpoints;

internal sealed class ChannelEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder api) =>
        api.MapGet("/channels", ListAsync)
            .WithTags("Channels")
            .RequireAuthorization()
            .Produces<IReadOnlyList<ChannelDto>>();

    private static async Task<IResult> ListAsync(ListChannels.Handler handler, CancellationToken ct) =>
        TypedResults.Ok(await handler.HandleAsync(ct));
}
