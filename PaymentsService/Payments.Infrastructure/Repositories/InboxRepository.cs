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
    public class InboxRepository : IInboxRepository
    {
        private readonly PaymentsDbContext _db;
        public InboxRepository(PaymentsDbContext db) => _db = db;

        public async Task<bool> ExistsAsync(Guid messageId, CancellationToken ct)
        {
            return await _db.InboxMessages.AnyAsync(x => x.MessageId == messageId, ct);
        }           

        public async Task SaveAsync(InboxMessage msg, CancellationToken ct)
        {
            await _db.InboxMessages.AddAsync(msg, ct);
        }           
    }
}
