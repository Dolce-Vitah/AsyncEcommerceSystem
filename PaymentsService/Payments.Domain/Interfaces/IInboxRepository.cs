using Payments.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Domain.Interfaces
{
    public interface IInboxRepository
    {
        Task<bool> ExistsAsync(Guid messageId, CancellationToken ct);
        Task SaveAsync(InboxMessage msg, CancellationToken ct);
    }
}
