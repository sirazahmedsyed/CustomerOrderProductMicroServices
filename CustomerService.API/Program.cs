using CustomerService.API.Infrastructure.Services;
using CustomerService.API.Infrastructure.UnitOfWork;
using CustomerService.API.Infrastructure.Profiles;
using Microsoft.EntityFrameworkCore;
using CustomerService.API.Infrastructure.DBContext;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CustomerService.API.Infrastructure.Middleware;
using SharedRepository.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedServices(builder.Configuration);
//#endregion
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICustomerService, CustomerServices>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5001, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
// Custom middleware to handle unauthorized access
app.UseMiddleware<CustomAuthenticationMiddleware>();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
