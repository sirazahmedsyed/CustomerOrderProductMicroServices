using UserService.API.Infrastructure.Services;
using UserService.API.Infrastructure.UnitOfWork;
using UserService.API.Infrastructure.Profiles;
using Microsoft.EntityFrameworkCore;
using UserService.API.Infrastructure.DBContext;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserService.API.Infrastructure.Middleware;
using SharedRepository.Repositories;
using UserService.API.Infrastructure.DBContext;
using Microsoft.AspNetCore.Authorization;
using SharedRepository.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedAuthorization(builder.Configuration);
builder.Services.AddAuthenticationSharedServices(builder.Configuration);
builder.Services.AddSwaggerGenSharedServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserServices>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddDbContext<UserDbContext>(options =>
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
