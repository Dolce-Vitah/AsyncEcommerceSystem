using Microsoft.EntityFrameworkCore;
using Orders.Domain.Interfaces;
using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Infrastructure.Database
{
    public class OrdersDbContext : DbContext, IUnitOfWork
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> opt) : base(opt) { }        

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Order>(b =>
            {
                b.HasKey(x => x.ID);
                b.Property(x => x.Status).HasConversion<string>();
            });
            mb.Entity<OutboxMessage>(b =>
            {
                b.HasKey(x => x.Id);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default) => base.SaveChangesAsync(ct);
    }
}
