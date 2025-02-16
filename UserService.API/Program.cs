using Microsoft.EntityFrameworkCore;
using SharedRepository.Authorization;
using SharedRepository.RedisCache;
using SharedRepository.Repositories;
using StackExchange.Redis;
using UserService.API.Infrastructure.DBContext;
using UserService.API.Infrastructure.Profiles;
using UserService.API.Infrastructure.Services;
using UserService.API.Infrastructure.UnitOfWork;
using RabbitMQHelper.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedAuthorization(builder.Configuration);
builder.Services.AddAuthenticationSharedServices(builder.Configuration);
builder.Services.AddSwaggerGenSharedServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserServices>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

// Add health checks
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis"), name: "redis", tags: new[] { "ready" });

builder.Services.AddHttpContextAccessor();
builder.Services.AddRabbitMQPublisher(builder.Configuration, config => { });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "UserGroupServiceAPI";
});

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
