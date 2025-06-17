using MediatR;
using Orders.Domain.Interfaces;
using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.UseCases.Queries
{
    public class GetOrderListQueryHandler : IRequestHandler<GetOrderListQuery, IEnumerable<Order>>
    {
        private readonly IOrderRepository _orders;
        public GetOrderListQueryHandler(IOrderRepository orders) => _orders = orders;
        public Task<IEnumerable<Order>> Handle(GetOrderListQuery rq, CancellationToken ct)
            => _orders.ListByUserAsync(rq.UserId, ct);
    }
}
