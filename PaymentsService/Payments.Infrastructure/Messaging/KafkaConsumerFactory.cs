using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Infrastructure.Messaging
{
    public class KafkaConsumerFactory
    {
        private readonly ConsumerConfig _config;
        private readonly IConfiguration _cfg;

        public KafkaConsumerFactory(ConsumerConfig config, IConfiguration cfg)
        {
            _config = config;
            _cfg = cfg;
        }

        public IConsumer<Ignore, string> CreateOrderCreatedConsumer()
        {
            var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
            consumer.Subscribe(_cfg["Kafka:Topic_OrderCreated"]!);
            return consumer;
        }
    }
}
