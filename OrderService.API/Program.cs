using GrpcClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OrderService.API.Infrastructure.DBContext;
using OrderService.API.Infrastructure.KafkaMessageBroker;
using OrderService.API.Infrastructure.Profiles;
using OrderService.API.Infrastructure.RabbitMQMessageBroker;
using OrderService.API.Infrastructure.Services;
using OrderService.API.Infrastructure.UnitOfWork;
using SharedRepository.Authorization;
using SharedRepository.Repositories;

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

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Register RabbitMQ Connection
builder.Services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();

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

// Register Kafka consumer service
builder.Services.AddHostedService<KafkaConsumerService>();

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
