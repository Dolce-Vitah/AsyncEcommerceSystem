using MediatR;
using Payments.Domain.Interfaces;
using Payments.UseCases.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.UseCases.Queries
{
    public class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, decimal?>
    {
        private readonly IAccountRepository _accounts;
        public GetBalanceQueryHandler(IAccountRepository accounts) => _accounts = accounts;

        public async Task<decimal?> Handle(GetBalanceQuery rq, CancellationToken ct)
        {
            var account = await _accounts.GetAsync(rq.AccountId, ct);
            if (account == null)
            {
                throw new NotFoundException($"Account with ID {rq.AccountId} doesn't exist.");
            }

            return await _accounts.GetBalanceAsync(rq.AccountId, ct);
        }
    }
}
