using MediatR;
using Orders.Domain.Interfaces;
using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Orders.UseCases.Commands
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderRepository _orders;
        private readonly IOutboxRepository _outbox;

        public CreateOrderCommandHandler(IOrderRepository orders, IOutboxRepository outbox)
        {
            _orders = orders;
            _outbox = outbox;
        }

        public async Task<Guid> Handle(CreateOrderCommand rq, CancellationToken ct)
        {
            if (rq.amount <= 0)
            {
                throw new InvalidOperationException("Order amount must be greater than zero.");
            }

            var order = new Order(rq.userId, rq.amount);
            await _orders.AddAsync(order, ct);

            var @event = new
            {
                OrderId = order.ID,
                UserId = order.UserId,
                Amount = order.Amount,
            };

            var payload = JsonSerializer.Serialize(@event);
            await _outbox.SaveAsync("OrderCreated", payload, ct);

            await _orders.UnitOfWork.SaveChangesAsync(ct);
            return order.ID;
        }
    }
}
