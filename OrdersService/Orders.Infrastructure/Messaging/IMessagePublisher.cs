using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Infrastructure.Messaging
{
    public interface IMessagePublisher
    {
        Task PublishAsync(string topic, string payload, CancellationToken ct);
    }
}
