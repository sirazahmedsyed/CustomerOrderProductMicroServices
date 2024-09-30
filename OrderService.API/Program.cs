using OrderService.API.Infrastructure.Services;
using OrderService.API.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Infrastructure.UnitOfWork;
using OrderService.API.Infrastructure.Profiles;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OrderService.API.Infrastructure.Middleware;
using SharedRepository.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderService, OrderServices>();

builder.Services.AddControllers();
// Configure the HTTP request pipeline.
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Custom middleware to handle unauthorized access
app.UseMiddleware<CustomAuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
