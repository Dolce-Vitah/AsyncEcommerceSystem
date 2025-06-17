using Microsoft.EntityFrameworkCore;
using Payments.Domain.Interfaces;
using Payments.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Infrastructure.Database
{
    public class PaymentsDbContext : DbContext, IUnitOfWork
    {
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> opt) : base(opt) { }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Account>().HasKey(x => x.ID);
            mb.Entity<Account>().HasIndex(x => x.UserId).IsUnique();

            mb.Entity<Transaction>().HasKey(x => x.Id);
            mb.Entity<Transaction>()
              .HasIndex(x => new { x.AccountId, x.OrderId })
              .IsUnique()
              .HasFilter("\"OrderId\" IS NOT NULL");

            mb.Entity<InboxMessage>().HasKey(x => x.MessageId);
            mb.Entity<OutboxMessage>().HasKey(x => x.Id);
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default) => base.SaveChangesAsync(ct);
    }
}
