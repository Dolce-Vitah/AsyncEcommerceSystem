using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orders.Domain.Interfaces;
using Orders.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Infrastructure.HostedServices
{
    public class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(1);

        public OutboxProcessorService(IServiceProvider sp) => _sp = sp;

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                using var scope = _sp.CreateScope();
                var outbox = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                var ordersUow = scope.ServiceProvider.GetRequiredService<IOrderRepository>().UnitOfWork;
                var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

                var messages = await outbox.GetUnprocessedAsync(20, ct);
                if (messages.Count > 0)
                {
                    foreach (var msg in messages)
                    {
                        // публикуем в Kafka
                        await publisher.PublishAsync(msg.Type, msg.Payload, ct);
                        await outbox.MarkProcessedAsync(msg.Id, ct);
                    }
                    // фиксируем статус Outbox-Message
                    await ordersUow.SaveChangesAsync(ct);
                }

                await Task.Delay(_interval, ct);
            }
        }
    }
}
