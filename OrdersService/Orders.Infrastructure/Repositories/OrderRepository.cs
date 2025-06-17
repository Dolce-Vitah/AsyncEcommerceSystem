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
    public class OrderRepository : IOrderRepository
    {
        private readonly OrdersDbContext _db;
        public OrderRepository(OrdersDbContext db) => _db = db;
        public IUnitOfWork UnitOfWork => _db;
        public async Task AddAsync(Order o, CancellationToken ct)
        {
            await _db.Orders.AddAsync(o, ct);
        }

        public async Task<Order?> GetAsync(Guid id, CancellationToken ct)
        {
            return await _db.Orders.FindAsync(new object[] { id }, ct);
        }

        public async Task<IEnumerable<Order>> ListByUserAsync(Guid userId, CancellationToken ct)
        {
            return await _db.Orders
                .Where(o => o.UserId == userId)
                .ToListAsync(ct);
        }
    }
}
