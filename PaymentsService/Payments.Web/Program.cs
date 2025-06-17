using Confluent.Kafka;
using Confluent.Kafka.Admin;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payments.Domain.Interfaces;
using Payments.Infrastructure.Database;
using Payments.Infrastructure.HostedServices;
using Payments.Infrastructure.Messaging;
using Payments.Infrastructure.Repositories;
using Payments.UseCases.Commands;
using Payments.UseCases.Queries;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("kafka.settings.json", optional: false);
builder.Services.AddSingleton(new ProducerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
});
builder.Services.AddSingleton(new ConsumerConfig
{
    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
    GroupId = builder.Configuration["Kafka:GroupId"],
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = false
});

builder.Services.AddDbContext<PaymentsDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IInboxRepository, InboxRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommandHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TopUpAccountCommandHandler).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetBalanceQueryHandler).Assembly));

builder.Services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();
builder.Services.AddSingleton<KafkaConsumerFactory>();

builder.Services.AddHostedService<OrderCreatedConsumerService>();
builder.Services.AddHostedService<OutboxProcessorService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
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
                Name = "order.created", 
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