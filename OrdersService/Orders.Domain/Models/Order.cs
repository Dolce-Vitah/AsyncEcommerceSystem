using Orders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Domain.Models
{
    public class Order
    {
        public Guid ID { get; private set; }
        public Guid UserId { get; private set; }
        public decimal Amount { get; private set; }
        public OrderStatus Status { get; private set; }

        public Order(Guid userId, decimal amount)
        {
            ID = new Guid();
            UserId = userId;
            Amount = amount;
            Status = OrderStatus.Pending;
        }

        public void MarkPaid() => Status = OrderStatus.Paid;

        public void MarkFailed() => Status = OrderStatus.Failed;
    }
}
