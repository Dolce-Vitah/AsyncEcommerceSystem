using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Models;
using Orders.Infrastructure.Database;
using Orders.Infrastructure.Repositories;
using Xunit;

public class OutboxRepositoryTests
{
    private OrdersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new OrdersDbContext(options);
    }

    [Fact]
    public async Task SaveAsync_AddsMessageToDb()
    {
        using var db = CreateDbContext();
        var repo = new OutboxRepository(db);

        await repo.SaveAsync("TestType", "TestPayload", CancellationToken.None);
        await db.SaveChangesAsync();

        var msg = await db.OutboxMessages.FirstOrDefaultAsync();
        Assert.NotNull(msg);
        Assert.Equal("TestType", msg.Type);
        Assert.Equal("TestPayload", msg.Payload);
        Assert.False(msg.Processed);
    }

    [Fact]
    public async Task GetUnprocessedAsync_ReturnsOnlyUnprocessedMessages()
    {
        using var db = CreateDbContext();
        db.OutboxMessages.Add(new OutboxMessage { Type = "A", Payload = "1", Processed = false, OccurredAt = DateTime.UtcNow });
        db.OutboxMessages.Add(new OutboxMessage { Type = "B", Payload = "2", Processed = true, OccurredAt = DateTime.UtcNow });
        db.OutboxMessages.Add(new OutboxMessage { Type = "C", Payload = "3", Processed = false, OccurredAt = DateTime.UtcNow.AddMinutes(1) });
        await db.SaveChangesAsync();

        var repo = new OutboxRepository(db);
        var result = await repo.GetUnprocessedAsync(10, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, m => Assert.False(m.Processed));
        Assert.Equal("A", result[0].Type);
        Assert.Equal("C", result[1].Type);
    }

    [Fact]
    public async Task MarkProcessedAsync_SetsProcessedTrue_AndSaves()
    {
        using var db = CreateDbContext();
        var msg = new OutboxMessage { Type = "A", Payload = "1", Processed = false, OccurredAt = DateTime.UtcNow };
        db.OutboxMessages.Add(msg);
        await db.SaveChangesAsync();

        var repo = new OutboxRepository(db);
        await repo.MarkProcessedAsync(msg.Id, CancellationToken.None);

        var updated = await db.OutboxMessages.FindAsync(msg.Id);
        Assert.True(updated.Processed);
    }
}
