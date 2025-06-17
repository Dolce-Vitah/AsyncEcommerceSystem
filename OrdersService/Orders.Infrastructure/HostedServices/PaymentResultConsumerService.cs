using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orders.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Orders.Infrastructure.HostedServices
{
    public class PaymentResultConsumerService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ConsumerConfig _config;
        private readonly string _topicPaymentProcessed;

        public PaymentResultConsumerService(IServiceProvider sp, ConsumerConfig config, IConfiguration cfg)
        {
            _sp = sp;
            _config = config;
            _topicPaymentProcessed = cfg["Kafka:Topic_PaymentProcessed"]!;
        }

        protected override Task ExecuteAsync(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
                consumer.Subscribe(_topicPaymentProcessed);

                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        var cr = consumer.Consume(ct);
                        var evt = JsonSerializer.Deserialize<PaymentProcessedEvent>(cr.Message.Value)!;

                        using var scope = _sp.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                        var order = await repo.GetAsync(evt.OrderId, ct);
                        if (order != null)
                        {
                            if (evt.Success) order.MarkPaid();
                            else order.MarkFailed();
                            await repo.UnitOfWork.SaveChangesAsync(ct);
                        }

                        consumer.Commit(cr); 
                    }
                }
                catch (OperationCanceledException) { /* graceful shutdown */ }
                finally
                {
                    consumer.Close();
                }
            }, ct);
        }

        private record PaymentProcessedEvent(Guid OrderId, bool Success);
    }
}
