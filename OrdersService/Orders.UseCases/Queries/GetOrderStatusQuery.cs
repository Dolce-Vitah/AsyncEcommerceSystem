using MediatR;
using Orders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.UseCases.Queries
{
    public record GetOrderStatusQuery(Guid OrderId) : IRequest<OrderStatus?>;
}
