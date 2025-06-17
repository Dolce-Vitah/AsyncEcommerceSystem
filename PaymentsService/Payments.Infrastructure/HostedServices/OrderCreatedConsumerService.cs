using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payments.Domain.Interfaces;
using Payments.Domain.Models;
using Payments.Infrastructure.Database;
using Payments.Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Payments.Infrastructure.HostedServices
{
    public class OrderCreatedConsumerService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly KafkaConsumerFactory _factory;

        public OrderCreatedConsumerService(IServiceProvider sp, KafkaConsumerFactory factory)
        {
            _sp = sp;
            _factory = factory;
        }

        protected override Task ExecuteAsync(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                using var consumer = _factory.CreateOrderCreatedConsumer();
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        var cr = consumer.Consume(ct);
                        var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(cr.Message.Value)!;
                        using var scope = _sp.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                        var inbox = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
                        var acctRepo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
                        var outbox = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

                        await using var tx = await db.Database.BeginTransactionAsync(ct);

                        if (!await inbox.ExistsAsync(evt.OrderId, ct))
                        {
                            await inbox.SaveAsync(new InboxMessage
                            {
                                MessageId = evt.OrderId,
                                Type = "OrderCreated",
                                Payload = cr.Message.Value
                            }, ct);

                            var account = await acctRepo.GetByUserAsync(evt.UserId, ct);
                            bool success = false;
                            if (account != null)
                            {
                                var balance = await acctRepo.GetBalanceAsync(account.ID, ct) ?? 0m;
                                if (balance >= evt.Amount)
                                {
                                    await acctRepo.AddTransactionAsync(account.ID, evt.OrderId, -evt.Amount, ct);
                                    success = true;
                                }
                            }

                            var resultEvt = new
                            {
                                OrderId = evt.OrderId,
                                Success = success
                            };
                            await outbox.SaveAsync(new OutboxMessage
                            {
                                Id = Guid.NewGuid(),
                                Type = "PaymentProcessed",
                                Payload = JsonSerializer.Serialize(resultEvt)
                            }, ct);

                            await db.SaveChangesAsync(ct);
                            await tx.CommitAsync(ct);
                        }

                        consumer.Commit(cr);
                    }
                }
                catch (OperationCanceledException) { }
                finally { consumer.Close(); }
            }, ct);
        }

        private record OrderCreatedEvent(Guid OrderId, Guid UserId, decimal Amount);
    }
}
