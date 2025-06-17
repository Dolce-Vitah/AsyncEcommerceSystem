using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Models;
using Payments.Infrastructure.Database;
using Payments.Infrastructure.Repositories;
using Xunit;

public class InboxRepositoryTests
{
    private PaymentsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PaymentsDbContext(options);
    }

    [Fact]
    public async Task ExistsAsync_Returns_True_If_Message_Exists()
    {
        using var db = CreateDbContext();
        var messageId = Guid.NewGuid();
        db.InboxMessages.Add(new InboxMessage
        {
            MessageId = messageId,
            Type = "Test",
            Payload = "{}",
            Processed = false,
            ReceivedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var repo = new InboxRepository(db);
        var exists = await repo.ExistsAsync(messageId, CancellationToken.None);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_Returns_False_If_Message_Does_Not_Exist()
    {
        using var db = CreateDbContext();
        var repo = new InboxRepository(db);
        var exists = await repo.ExistsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(exists);
    }

    [Fact]
    public async Task SaveAsync_Adds_Message()
    {
        using var db = CreateDbContext();
        var repo = new InboxRepository(db);
        var msg = new InboxMessage
        {
            MessageId = Guid.NewGuid(),
            Type = "Test",
            Payload = "{}",
            Processed = false,
            ReceivedAt = DateTime.UtcNow
        };

        await repo.SaveAsync(msg, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.Single(db.InboxMessages);
        Assert.Equal(msg.MessageId, db.InboxMessages.FirstAsync().Result.MessageId);
    }
}

