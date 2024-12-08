using GrpcClient;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Infrastructure.DBContext;
using OrderService.API.Infrastructure.Profiles;
using OrderService.API.Infrastructure.Services;
using OrderService.API.Infrastructure.UnitOfWork;
using SharedRepository.Authorization;
using SharedRepository.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSharedAuthorization(builder.Configuration);
builder.Services.AddAuthenticationSharedServices(builder.Configuration);
builder.Services.AddSwaggerGenSharedServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderService, OrderServices>();
builder.Services.AddSingleton<ProductDetailsClient>();
builder.Services.AddSingleton<InactiveFlagClient>();
builder.Services.AddSingleton<CustomerClient>();
builder.Services.AddScoped<IDataAccessHelper, DataAccessHelper>();

builder.Services.AddControllers();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UsePermissionMiddleware();
app.UseAuthorization();
app.MapControllers();
app.Run();
