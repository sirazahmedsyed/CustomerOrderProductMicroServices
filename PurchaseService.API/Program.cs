using Microsoft.EntityFrameworkCore;
using PurchaseService.API.Infrastructure.DBContext;
using PurchaseService.API.Infrastructure.Services;
using PurchaseService.API.Infrastructure.Profiles;
using PurchaseService.API.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using SharedRepository.Repositories;
using SharedRepository.Authorization;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using PurchaseService.API.Infrastructure.Services;
using PurchaseService.API.Infrastructure.DBContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedAuthorization(builder.Configuration);
builder.Services.AddAuthenticationSharedServices(builder.Configuration);
builder.Services.AddSwaggerGenSharedServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPurchaseService, PurchaseServices>();
builder.Services.AddScoped<IDataAccessHelper, DataAccessHelper>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddDbContext<PurchaseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
app.UseCors("CorsPolicy");
app.UseAuthentication();
//app.UseMiddleware<CustomAuthenticationMiddleware>();
app.UsePermissionMiddleware();
app.UseAuthorization();
app.MapControllers();
app.Run();
