using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Orders.Domain.Interfaces;
using Orders.Domain.Models;
using Orders.Infrastructure.Messaging;
using Orders.Infrastructure.HostedServices;
using Xunit;

public class OutboxProcessorServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ProcessesUnprocessedMessages()
    {
        // Arrange
        var outboxRepo = new Mock<IOutboxRepository>();
        var orderUow = new Mock<IUnitOfWork>();
        var orderRepo = new Mock<IOrderRepository>();
        var publisher = new Mock<IMessagePublisher>();

        var messages = new List<OutboxMessage>
        {
            new OutboxMessage { Id = Guid.NewGuid(), Type = "OrderCreated", Payload = "payload", Processed = false }
        };

        outboxRepo.Setup(r => r.GetUnprocessedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(messages);
        orderRepo.SetupGet(r => r.UnitOfWork).Returns(orderUow.Object);

        var serviceProvider = new Mock<IServiceProvider>();
        var scope = new Mock<IServiceScope>();
        var scopeFactory = new Mock<IServiceScopeFactory>();

        serviceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
                       .Returns(scopeFactory.Object);

        scope.Setup(s => s.ServiceProvider).Returns(serviceProvider.Object);
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        serviceProvider.Setup(sp => sp.GetService(typeof(IOutboxRepository)))
               .Returns(outboxRepo.Object);
        serviceProvider.Setup(sp => sp.GetService(typeof(IOrderRepository)))
                       .Returns(orderRepo.Object);
        serviceProvider.Setup(sp => sp.GetService(typeof(IMessagePublisher)))
                       .Returns(publisher.Object);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100); 

        var service = new OutboxProcessorService(serviceProvider.Object);

        // Act
        await service.StartAsync(cts.Token);

        // Assert
        outboxRepo.Verify(r => r.GetUnprocessedAsync(20, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        publisher.Verify(p => p.PublishAsync("OrderCreated", "payload", It.IsAny<CancellationToken>()), Times.Once);
        outboxRepo.Verify(r => r.MarkProcessedAsync(messages[0].Id, It.IsAny<CancellationToken>()), Times.Once);
        orderUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
