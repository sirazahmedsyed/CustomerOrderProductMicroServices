using UserGroupService.API.Infrastructure.UnitOfWork;
using UserGroupService.API.Infrastructure.Profiles;
using Microsoft.EntityFrameworkCore;
using UserGroupService.API.Infrastructure.DBContext;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserGroupService.API.Infrastructure.Middleware;
using SharedRepository.Repositories;
using SharedRepository.Authorization;
using Microsoft.AspNetCore.Authorization;
using UserGroupService.API.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using GrpcClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedAuthorization(builder.Configuration);
builder.Services.AddAuthenticationSharedServices(builder.Configuration);
builder.Services.AddSwaggerGenSharedServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserGroupService, UserGroupServices>();
builder.Services.AddSingleton<InactiveFlagClient>();
builder.Services.AddScoped<IDataAccessHelper, DataAccessHelper>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddDbContext<UserGroupDbContext>(options =>
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
