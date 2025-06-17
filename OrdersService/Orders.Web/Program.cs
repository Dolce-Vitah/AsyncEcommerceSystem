using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Orders.Domain.Interfaces;
using Orders.Infrastructure.Database;
using Orders.Infrastructure.HostedServices;
using Orders.Infrastructure.Messaging;
using Orders.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Confluent.Kafka.Admin;
using Orders.UseCases.Commands;
using Orders.UseCases.Queries;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("kafka.settings.json", optional: false);
builder.Services.AddSingleton(provider =>
   new ProducerConfig
   {
       BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
   });
builder.Services.AddSingleton(provider =>
   new ConsumerConfig
   {
       BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
       GroupId = builder.Configuration["Kafka:GroupId"],
       AutoOffsetReset = AutoOffsetReset.Earliest,
       EnableAutoCommit = false
   });

builder.Services.AddDbContext<OrdersDbContext>(opt =>
   opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));  

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetOrderListQueryHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetOrderStatusQueryHandler).Assembly));

builder.Services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();

builder.Services.AddHostedService<OutboxProcessorService>();
builder.Services.AddHostedService<PaymentResultConsumerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();

    var config = new AdminClientConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
    };
    using var adminClient = new AdminClientBuilder(config).Build();
    try
    {
        adminClient.CreateTopicsAsync(new[]
        {
            new TopicSpecification
            {
                Name = "payment.processed",
                NumPartitions = 1,
                ReplicationFactor = 1
            }
        }).Wait();
    }
    catch (AggregateException ex) when (ex.InnerException is CreateTopicsException cte && cte.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
    {
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
