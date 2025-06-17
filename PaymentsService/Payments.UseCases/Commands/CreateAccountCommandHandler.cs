using MediatR;
using Payments.Domain.Interfaces;
using Payments.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.UseCases.Commands
{
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Guid>
    {
        private readonly IAccountRepository _accounts;
        public CreateAccountCommandHandler(IAccountRepository accounts) => _accounts = accounts;

        public async Task<Guid> Handle(CreateAccountCommand rq, CancellationToken ct)
        {
            if (await _accounts.GetByUserAsync(rq.UserId, ct) != null)
                throw new InvalidOperationException("Account already exists");

            var acct = new Account(rq.UserId);
            await _accounts.AddAsync(acct, ct);
            await _accounts.UnitOfWork.SaveChangesAsync(ct);
            return acct.ID;
        }
    }
}
