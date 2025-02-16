using AuditSrvice.API.Infrastructure.Consumers;
using AuditSrvice.API.Infrastructure.DBContext;
using AuditSrvice.API.Infrastructure.DTOs;
using AuditSrvice.API.Infrastructure.Repository;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQHelper.Infrastructure.Configuration;
using SharedRepository.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DbContext, ApplicationDbContext>();

builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AuditConsumer>();
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitConfig = builder.Configuration.GetSection("RabbitMQConfiguration").Get<RabbitMQConfiguration>();

        cfg.Host(new Uri($"rabbitmq://{rabbitConfig.Host}:{rabbitConfig.Port}"), host =>
        {
            host.Username(rabbitConfig.Username);
            host.Password(rabbitConfig.Password);
        });

        //// Configure the message topology for AuditMessageDto to use the audit-exchange
        //cfg.MessageTopology.GetMessageTopology<AuditMessageDto>()
        //    .SetEntityName(rabbitConfig.AuditExchangeName);

        //cfg.ReceiveEndpoint(rabbitConfig.AuditQueueName, e =>
        //{
        //    e.ConfigureConsumeTopology = false; // Disable automatic topology
        //    e.PublishFaults = false;
        //    e.Durable = true;
        //    e.AutoDelete = false;

        //    // Bind the queue to the audit-exchange with routing key
        //    e.Bind(rabbitConfig.AuditExchangeName, b =>
        //    {
        //        b.ExchangeType = ExchangeType.Direct; // Changed from Fanout to Direct
        //        b.Durable = true;
        //        b.AutoDelete = false;
        //        b.ExchangeType = ExchangeType.Direct;
        //        b.RoutingKey = "audit.message";
        //    });

        //    e.ConfigureConsumer<AuditConsumer>(context);

        //    //e.UseMessageRetry(r =>
        //    //   r.Intervals(TimeSpan.FromSeconds(1),
        //    //             TimeSpan.FromSeconds(5),
        //    //             TimeSpan.FromSeconds(10)));
        //});





        // Configure the audit-queue to bind to audit-exchange
        cfg.ReceiveEndpoint(rabbitConfig.AuditQueueName, e =>
        {
            e.ConfigureConsumeTopology = false; // Disable auto-bind
            e.Bind(rabbitConfig.AuditExchangeName, b =>
            {
                b.ExchangeType = ExchangeType.Direct;
                b.RoutingKey = "audit.message"; // Match publisher's routing key
            });
            e.ConfigureConsumer<AuditConsumer>(context);

            // Optional: Retry policy
            e.UseMessageRetry(r => r.Interval(3, 1000));
        });
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
