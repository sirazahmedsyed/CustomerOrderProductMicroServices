using GrpcClient;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Infrastructure.DBContext;
using OrderService.API.Infrastructure.KafkaMessageBroker;
using OrderService.API.Infrastructure.Profiles;
using OrderService.API.Infrastructure.RabbitMQMessageBroker;
using OrderService.API.Infrastructure.RedisMessageBroker;
using OrderService.API.Infrastructure.Services;
using OrderService.API.Infrastructure.UnitOfWork;
using SharedRepository.Authorization;
using SharedRepository.Repositories;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSharedAuthorization(builder.Configuration);
builder.Services.AddAuthenticationSharedServices(builder.Configuration);
builder.Services.AddSwaggerGenSharedServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure RabbitMQ Settings
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQSettings"));

// Configure Kafka settings
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));

// Configure Redis settings
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));
builder.Services.Configure<RedisChannelSettings>(builder.Configuration.GetSection("RedisChannelSettings"));


builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Register RabbitMQ Connection
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();

// Register Redis Connection Multiplexer
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var redisSettings = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RedisSettings>>().Value;
    var configurationOptions = ConfigurationOptions.Parse(redisSettings.ConnectionString);
    return ConnectionMultiplexer.Connect(configurationOptions);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOrderService, OrderServices>();
builder.Services.AddSingleton<ProductDetailsClient>();
builder.Services.AddSingleton<InactiveFlagClient>();
builder.Services.AddSingleton<CustomerClient>();
builder.Services.AddScoped<IDataAccessHelper, DataAccessHelper>();

// Register Generic Message Publisher
//builder.Services.AddTransient(typeof(IMessagePublisher<>), typeof(RabbitMQMessagePublisher<>));
builder.Services.AddScoped(typeof(IMessagePublisher<>), typeof(RabbitMQMessagePublisher<>));

// Register Kafka publisher
builder.Services.AddSingleton(typeof(IKafkaMessagePublisher<>), typeof(KafkaMessagePublisher<>));

// Register Redis publisher and Channel Configuration
builder.Services.AddSingleton(typeof(IRedisMessagePublisher<>), typeof(RedisMessagePublisher<>));

// Register Kafka consumer service
builder.Services.AddHostedService<KafkaConsumerService>();

// Register Redis consumer service
builder.Services.AddHostedService<RedisSubscriberService>();

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
