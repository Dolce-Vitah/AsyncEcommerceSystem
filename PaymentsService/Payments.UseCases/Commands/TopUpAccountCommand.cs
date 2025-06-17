using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.UseCases.Commands
{
    public record TopUpAccountCommand(Guid AccountId, decimal Amount) : IRequest<Unit>;
}
