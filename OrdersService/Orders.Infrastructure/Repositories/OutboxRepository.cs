using Microsoft.EntityFrameworkCore;
using Orders.Domain.Interfaces;
using Orders.Domain.Models;
using Orders.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Infrastructure.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly OrdersDbContext _db;
        public OutboxRepository(OrdersDbContext db) => _db = db;

        public async Task SaveAsync(string type, string payload, CancellationToken ct)
        {
            var msg = new OutboxMessage { Type = type, Payload = payload };
            await _db.OutboxMessages.AddAsync(msg, ct);
        }

        public Task<List<OutboxMessage>> GetUnprocessedAsync(int maxCount, CancellationToken ct)
            => _db.OutboxMessages
                  .Where(m => !m.Processed)
                  .OrderBy(m => m.OccurredAt)
                  .Take(maxCount)
                  .ToListAsync(ct);

        public async Task MarkProcessedAsync(Guid id, CancellationToken ct)
        {
            var msg = await _db.OutboxMessages.FindAsync(new object[] { id }, ct);
            if (msg != null) { msg.Processed = true; await _db.SaveChangesAsync(ct); }
        }
    }
}
