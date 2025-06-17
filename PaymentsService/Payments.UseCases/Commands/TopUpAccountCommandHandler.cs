using MediatR;
using Payments.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.UseCases.Commands
{
    public class TopUpAccountCommandHandler : IRequestHandler<TopUpAccountCommand, Unit>
    {
        private readonly IAccountRepository _accounts;
        public TopUpAccountCommandHandler(IAccountRepository accounts) => _accounts = accounts;

        public async Task<Unit> Handle(TopUpAccountCommand rq, CancellationToken ct)
        {
            var account = await _accounts.GetAsync(rq.AccountId, ct);

            if (account == null)
            {
                throw new NotFoundException($"Account with ID {rq.AccountId} doesn't exist.");
            }

            if (rq.Amount <= 0)
            {
                throw new ArgumentException("The amount must be greater than zero");
            }

            await _accounts.AddTransactionAsync(rq.AccountId, null, rq.Amount, ct);
            await _accounts.UnitOfWork.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
