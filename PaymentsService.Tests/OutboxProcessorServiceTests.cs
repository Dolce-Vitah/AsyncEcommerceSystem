using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Payments.Domain.Interfaces;
using Payments.Domain.Models;
using Payments.Infrastructure.Database;
using Payments.Infrastructure.HostedServices;
using Payments.Infrastructure.Messaging;
using Xunit;
using Microsoft.EntityFrameworkCore;

public class OutboxProcessorServiceTests
{
    [Fact]
    public async Task ExecuteAsync_Processes_Unprocessed_Outbox_Messages()
    {
        // Arrange
        var outboxMsg = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "TestType",
            Payload = "TestPayload"
        };
        var outboxMock = new Mock<IOutboxRepository>();
        outboxMock.Setup(o => o.GetUnprocessedAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OutboxMessage> { outboxMsg });
        outboxMock.Setup(o => o.MarkProcessedAsync(outboxMsg.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var publisherMock = new Mock<IMessagePublisher>();
        publisherMock.Setup(p => p.PublishAsync(outboxMsg.Type, outboxMsg.Payload, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var dbContextMock = new Mock<PaymentsDbContext>(new DbContextOptions<PaymentsDbContext>());
        dbContextMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var spMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.SetupGet(s => s.ServiceProvider).Returns(spMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
        spMock.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactoryMock.Object);

        spMock.Setup(s => s.GetService(typeof(IOutboxRepository))).Returns(outboxMock.Object);
        spMock.Setup(s => s.GetService(typeof(PaymentsDbContext))).Returns(dbContextMock.Object);
        spMock.Setup(s => s.GetService(typeof(IMessagePublisher))).Returns(publisherMock.Object);

        var service = new OutboxProcessorService(spMock.Object);

        // Act
        var cts = new CancellationTokenSource();
        cts.CancelAfter(150);
        await service.StartAsync(cts.Token);

        // Assert
        outboxMock.Verify(o => o.GetUnprocessedAsync(20, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        publisherMock.Verify(p => p.PublishAsync(outboxMsg.Type, outboxMsg.Payload, It.IsAny<CancellationToken>()), Times.Once);
        outboxMock.Verify(o => o.MarkProcessedAsync(outboxMsg.Id, It.IsAny<CancellationToken>()), Times.Once);
        dbContextMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
