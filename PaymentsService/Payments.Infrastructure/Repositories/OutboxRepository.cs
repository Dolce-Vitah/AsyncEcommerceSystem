using Microsoft.EntityFrameworkCore;
using Payments.Domain.Interfaces;
using Payments.Domain.Models;
using Payments.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Infrastructure.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly PaymentsDbContext _db;
        public OutboxRepository(PaymentsDbContext db) => _db = db;

        public async Task SaveAsync(OutboxMessage msg, CancellationToken ct)
        {
            await _db.OutboxMessages.AddAsync(msg, ct);
        }

        public async Task<List<OutboxMessage>> GetUnprocessedAsync(int maxCount, CancellationToken ct)
        {
            return await _db.OutboxMessages
                  .Where(x => !x.Processed)
                  .OrderBy(x => x.OccurredAt)
                  .Take(maxCount)
                  .ToListAsync(ct);
        }           

        public async Task MarkProcessedAsync(Guid id, CancellationToken ct)
        {
            var m = await _db.OutboxMessages.FindAsync(new object[] { id }, ct);
            if (m != null) { m.Processed = true; }
        }
    }
}
