using GrpcClient;
using Microsoft.EntityFrameworkCore;
using PurchaseService.API.Infrastructure.DBContext;
using PurchaseService.API.Infrastructure.Profiles;
using PurchaseService.API.Infrastructure.Services;
using PurchaseService.API.Infrastructure.UnitOfWork;
using SharedRepository.Authorization;
using SharedRepository.Repositories;
using SharedRepository.RabbitMQMessageBroker.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedAuthorization(builder.Configuration);
builder.Services.AddAuthenticationSharedServices(builder.Configuration);
builder.Services.AddSwaggerGenSharedServices(builder.Configuration);
builder.Services.AddSharedRabbitMQ();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPurchaseService, PurchaseServices>();
builder.Services.AddSingleton<InactiveFlagClient>();
builder.Services.AddSingleton<ProductDetailsClient>();
builder.Services.AddSingleton<CustomerClient>();
builder.Services.AddScoped<IDataAccessHelper, DataAccessHelper>();
// Program.cs in OrderService.API or PurchaseService.API

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
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UsePermissionMiddleware();
app.UseAuthorization();
app.MapControllers();
app.Run();
