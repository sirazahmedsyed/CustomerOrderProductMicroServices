using Microsoft.EntityFrameworkCore;
using ProductService.API.Infrastructure.DBContext;
using ProductService.API.Infrastructure.Profiles;
using ProductService.API.Infrastructure.Services;
using ProductService.API.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ProductService.API.Infrastructure.Middleware;
using SharedRepository.Repositories;
using SharedRepository.Authorization;
using ProductService.API.Infrastructure.Authorization;
using System.Net;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy(Permissions.AddUser, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AddUser)));
//    options.AddPolicy(Permissions.AddUserGroup, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AddUserGroup)));
//    options.AddPolicy(Permissions.AddCustomer, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AddCustomer)));
//    options.AddPolicy(Permissions.AddProducts, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AddProducts)));
//    options.AddPolicy(Permissions.AddOrder, policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AddOrder)));
//});


//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("RequirePermissions", policy =>
//        policy.Requirements.Add(new PermissionRequirement(Permissions.AddProducts)));
//});
builder.Services.AddSharedAuthorization(builder.Configuration);
builder.Services.AddAuthenticationSharedServices(builder.Configuration);
builder.Services.AddSwaggerGenSharedServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductServices>();
//builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5003, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
// Custom middleware to handle unauthorized access
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseMiddleware<CustomAuthenticationMiddleware>();
app.UsePermissionMiddleware();
app.UseAuthorization();
app.MapControllers();
app.Run();

