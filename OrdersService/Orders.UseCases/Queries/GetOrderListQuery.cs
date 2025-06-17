using MediatR;
using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.UseCases.Queries
{
    public record GetOrderListQuery(Guid UserId) : IRequest<IEnumerable<Order>>;
}
