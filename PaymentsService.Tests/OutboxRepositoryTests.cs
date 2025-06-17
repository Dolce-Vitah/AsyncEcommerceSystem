using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Models;
using Payments.Infrastructure.Database;
using Payments.Infrastructure.Repositories;
using Xunit;

public class OutboxRepositoryTests
{
    private PaymentsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PaymentsDbContext(options);
    }

    [Fact]
    public async Task SaveAsync_Adds_OutboxMessage()
    {
        using var db = CreateDbContext();
        var repo = new OutboxRepository(db);
        var msg = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "TestType",
            Payload = "{}",
            Processed = false,
            OccurredAt = DateTime.UtcNow
        };

        await repo.SaveAsync(msg, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.Single(db.OutboxMessages);
        Assert.Equal(msg.Id, db.OutboxMessages.First().Id);
    }

    [Fact]
    public async Task GetUnprocessedAsync_Returns_Unprocessed_Messages_Ordered()
    {
        using var db = CreateDbContext();
        var msg1 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "A",
            Payload = "1",
            Processed = false,
            OccurredAt = DateTime.UtcNow.AddMinutes(-2)
        };
        var msg2 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "B",
            Payload = "2",
            Processed = false,
            OccurredAt = DateTime.UtcNow.AddMinutes(-1)
        };
        var msg3 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "C",
            Payload = "3",
            Processed = true,
            OccurredAt = DateTime.UtcNow
        };
        db.OutboxMessages.AddRange(msg1, msg2, msg3);
        await db.SaveChangesAsync();

        var repo = new OutboxRepository(db);
        var result = await repo.GetUnprocessedAsync(10, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(msg1.Id, result[0].Id);
        Assert.Equal(msg2.Id, result[1].Id);
    }

    [Fact]
    public async Task MarkProcessedAsync_Sets_Processed_True()
    {
        using var db = CreateDbContext();
        var msg = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "Test",
            Payload = "payload",
            Processed = false,
            OccurredAt = DateTime.UtcNow
        };
        db.OutboxMessages.Add(msg);
        await db.SaveChangesAsync();

        var repo = new OutboxRepository(db);
        await repo.MarkProcessedAsync(msg.Id, CancellationToken.None);
        await db.SaveChangesAsync();

        var updated = await db.OutboxMessages.FindAsync(msg.Id);
        Assert.True(updated.Processed);
    }
}
