using CustomerService.API.Infrastructure.DBContext;
using CustomerService.API.Infrastructure.Profiles;
using CustomerService.API.Infrastructure.Services;
using CustomerService.API.Infrastructure.UnitOfWork;
using GrpcClient;
using Microsoft.EntityFrameworkCore;
using SharedRepository.Authorization;
using SharedRepository.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSharedAuthorization(builder.Configuration);
builder.Services.AddAuthenticationSharedServices(builder.Configuration);
builder.Services.AddSwaggerGenSharedServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICustomerService, CustomerServices>();
builder.Services.AddSingleton<InactiveFlagClient>();
builder.Services.AddSingleton<ProductDetailsClient>();
builder.Services.AddSingleton<CustomerClient>();
builder.Services.AddScoped<IDataAccessHelper, DataAccessHelper>();

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

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UsePermissionMiddleware();
app.UseAuthorization();
app.MapControllers();
app.Run();
