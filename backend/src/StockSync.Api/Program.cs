using StockSync.Api.Configuration;
using StockSync.Api.Endpoints;
using StockSync.Application;
using StockSync.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure();
builder.AddApi();

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseApiPipeline();
app.MapApiEndpoints();

await app.RunAsync();

public partial class Program;
