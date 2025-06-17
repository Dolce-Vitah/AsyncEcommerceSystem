using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task AddAsync(Order o, CancellationToken ct);
        Task<Order?> GetAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<Order>> ListByUserAsync(Guid userId, CancellationToken ct);
        IUnitOfWork UnitOfWork { get; }
    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
