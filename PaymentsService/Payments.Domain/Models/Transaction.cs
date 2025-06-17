using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Domain.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid? OrderId { get; set; }       
        public decimal Amount { get; set; }      
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
