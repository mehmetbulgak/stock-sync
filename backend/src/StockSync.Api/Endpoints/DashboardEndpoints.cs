using StockSync.Application.Features.Dashboard;

namespace StockSync.Api.Endpoints;

internal sealed class DashboardEndpoints : IEndpointGroup
{
    public void Map(IEndpointRouteBuilder api) =>
        api.MapGet("/dashboard", GetAsync)
            .WithTags("Dashboard")
            .RequireAuthorization()
            .Produces<DashboardDto>();

    private static async Task<IResult> GetAsync(GetDashboard.Handler handler, CancellationToken ct) =>
        TypedResults.Ok(await handler.HandleAsync(ct));
}
