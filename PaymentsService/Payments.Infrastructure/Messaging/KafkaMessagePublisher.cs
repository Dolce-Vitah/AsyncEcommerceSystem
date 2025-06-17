using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Infrastructure.Messaging
{
    public class KafkaMessagePublisher : IMessagePublisher
    {
        private readonly IProducer<Null, string> _producer;
        private readonly IConfiguration _cfg;

        public KafkaMessagePublisher(ProducerConfig cfg, IConfiguration configuration)
        {
            _producer = new ProducerBuilder<Null, string>(cfg).Build();
            _cfg = configuration;
        }

        public async Task PublishAsync(string topicKey, string payload, CancellationToken ct)
        {
            var topic = topicKey switch
            {
                "PaymentProcessed" => _cfg["Kafka:Topic_PaymentProcessed"],
                _ => throw new ArgumentException($"Unknown topic: {topicKey}")
            };
            await _producer.ProduceAsync(topic!,
                new Message<Null, string> { Value = payload }, ct);
        }
    }
}
