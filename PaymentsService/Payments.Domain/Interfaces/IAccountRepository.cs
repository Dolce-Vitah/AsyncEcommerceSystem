using Payments.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Domain.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByUserAsync(Guid userId, CancellationToken ct);
        Task<Account?> GetAsync(Guid accountId, CancellationToken ct);
        Task AddAsync(Account acct, CancellationToken ct);
        Task AddTransactionAsync(Guid accountId, Guid? orderId, decimal amount, CancellationToken ct);
        Task<decimal?> GetBalanceAsync(Guid accountId, CancellationToken ct);
        IUnitOfWork UnitOfWork { get; }
    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
