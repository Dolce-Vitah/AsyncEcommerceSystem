using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Infrastructure.Messaging
{
    public class KafkaMessagePublisher : IMessagePublisher
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topicOrderCreated;

        public KafkaMessagePublisher(ProducerConfig config, IConfiguration cfg)
        {
            _producer = new ProducerBuilder<Null, string>(config).Build();
            _topicOrderCreated = cfg["Kafka:Topic_OrderCreated"]!;
        }

        public async Task PublishAsync(string topic, string payload, CancellationToken ct)
        {
            var t = topic switch
            {
                "OrderCreated" => _topicOrderCreated,
                _ => throw new ArgumentException($"Unknown topic key: {topic}")
            };

            var message = new Message<Null, string> { Value = payload };
            await _producer.ProduceAsync(t, message, ct);
        }
    }
}
