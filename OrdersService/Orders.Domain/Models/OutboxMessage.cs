using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Domain.Models
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = default!;
        public string Payload { get; set; } = default!;
        public bool Processed { get; set; } = false;
    }
}
