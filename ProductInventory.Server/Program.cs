using ProductInventory.Server.Repositories;
using ProductInventory.Server.Services;
using ProductInventory.Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddSingleton<IProductRepository, ProductRepository>();

var app = builder.Build();

app.MapGrpcService<InventoryService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
