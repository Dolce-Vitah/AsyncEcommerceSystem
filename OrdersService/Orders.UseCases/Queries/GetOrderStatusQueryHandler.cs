using MediatR;
using Orders.Domain.Interfaces;
using Orders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.UseCases.Queries
{
    public class GetOrderStatusQueryHandler : IRequestHandler<GetOrderStatusQuery, OrderStatus?>
    {
        private readonly IOrderRepository _orders;
        public GetOrderStatusQueryHandler(IOrderRepository orders) => _orders = orders;
        public async Task<OrderStatus?> Handle(GetOrderStatusQuery rq, CancellationToken ct)
        {
            var o = await _orders.GetAsync(rq.OrderId, ct);

            if (o == null)
            {
                throw new NotFoundException($"There is no order with an ID {rq.OrderId}");
            }

            return o?.Status;
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
