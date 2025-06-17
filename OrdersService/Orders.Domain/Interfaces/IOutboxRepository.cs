using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Domain.Interfaces
{
    public interface IOutboxRepository
    {
        Task SaveAsync(string type, string payload, CancellationToken ct);
        Task<List<OutboxMessage>> GetUnprocessedAsync(int maxCount, CancellationToken ct);
        Task MarkProcessedAsync(Guid id, CancellationToken ct);
    }
}
