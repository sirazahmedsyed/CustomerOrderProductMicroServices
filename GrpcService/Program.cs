using GrpcService.Repository;
using GrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton<IInactiveFlagRepository, InactiveFlagRepository>();
builder.Services.AddSingleton<IProductDetailsRepository, ProductDetailsRepository>();
builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GrpcInactiveFlagSvc>();
app.MapGrpcService<GrpcInactiveCustomerFlagSvc>();
app.MapGrpcService<GrpcProductDetailsService>();
app.MapGrpcService<GrpcCustomerService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
