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
    public class AccountRepository : IAccountRepository
    {
        private readonly PaymentsDbContext _db;
        public AccountRepository(PaymentsDbContext db) => _db = db;
        public IUnitOfWork UnitOfWork => _db;

        public async Task AddAsync(Account acct, CancellationToken ct)
        {
            await _db.Accounts.AddAsync(acct, ct);
        }

        public async Task<Account?> GetByUserAsync(Guid userId, CancellationToken ct)
        {
            return await _db.Accounts.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        }
           

        public async Task<Account?> GetAsync(Guid accountId, CancellationToken ct)
        {
            return await _db.Accounts.FindAsync(new object[] { accountId }, ct);
        }           

        public async Task AddTransactionAsync(Guid accountId, Guid? orderId, decimal amount, CancellationToken ct)
        {
            var txn = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                OrderId = orderId,
                Amount = amount
            };
            await _db.Transactions.AddAsync(txn, ct);
        }

        public async Task<decimal?> GetBalanceAsync(Guid accountId, CancellationToken ct)
        {
            return await _db.Transactions
                  .Where(t => t.AccountId == accountId)
                  .SumAsync(t => (decimal?)t.Amount, ct);
        }           
    }
}
