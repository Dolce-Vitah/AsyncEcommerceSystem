using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Orders.UseCases.Commands
{
    public record CreateOrderCommand(Guid userId, decimal amount) : IRequest<Guid>;    
}
