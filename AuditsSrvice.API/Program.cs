using AuditSrvice.API.Infrastructure.Consumers;
using AuditSrvice.API.Infrastructure.DBContext;
using AuditSrvice.API.Infrastructure.Repository;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedRepository.MassTransit;
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

//MassTransit configuration in program.cs is for your refence
builder.Services.AddCustomMassTransit(builder.Configuration, mt =>
{
    mt.AddConsumer<AuditSrvice.API.Infrastructure.Consumers.AuditConsumer>();
});


builder.Services.AddControllers();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.Run();
