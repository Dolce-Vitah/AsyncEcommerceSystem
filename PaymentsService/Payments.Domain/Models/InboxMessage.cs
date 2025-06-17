using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Domain.Models
{
    public class InboxMessage
    {
        public Guid MessageId { get; set; }        
        public string Type { get; set; } = default!;
        public string Payload { get; set; } = default!;
        public bool Processed { get; set; } = false;
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    }
}
