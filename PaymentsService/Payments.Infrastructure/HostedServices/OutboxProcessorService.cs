using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payments.Domain.Interfaces;
using Payments.Infrastructure.Database;
using Payments.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Infrastructure.HostedServices
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
                var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

                var msgs = await outbox.GetUnprocessedAsync(20, ct);
                foreach (var m in msgs)
                {
                    await publisher.PublishAsync(m.Type, m.Payload, ct);
                    await outbox.MarkProcessedAsync(m.Id, ct);
                }

                await db.SaveChangesAsync(ct);
                await Task.Delay(_interval, ct);
            }
        }
    }
}