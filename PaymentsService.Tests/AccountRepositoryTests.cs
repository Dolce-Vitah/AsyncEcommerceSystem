using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Payments.Domain.Models;
using Payments.Infrastructure.Database;
using Payments.Infrastructure.Repositories;
using Xunit;

public class AccountRepositoryTests
{
    private PaymentsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PaymentsDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Adds_Account()
    {
        using var db = CreateDbContext();
        var repo = new AccountRepository(db);
        var account = new Account(Guid.NewGuid());

        await repo.AddAsync(account, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.Single(db.Accounts);
        Assert.Equal(account.UserId, db.Accounts.First().UserId);
    }

    [Fact]
    public async Task GetByUserAsync_Returns_Account_If_Exists()
    {
        using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var account = new Account(userId);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var repo = new AccountRepository(db);
        var result = await repo.GetByUserAsync(userId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task GetAsync_Returns_Account_If_Exists()
    {
        using var db = CreateDbContext();
        var account = new Account(Guid.NewGuid());
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var repo = new AccountRepository(db);
        var result = await repo.GetAsync(account.ID, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(account.ID, result.ID);
    }

    [Fact]
    public async Task AddTransactionAsync_Adds_Transaction()
    {
        using var db = CreateDbContext();
        var account = new Account(Guid.NewGuid());
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        var repo = new AccountRepository(db);
        var orderId = Guid.NewGuid();
        await repo.AddTransactionAsync(account.ID, orderId, 50m, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.Single(db.Transactions);
        Assert.Equal(50m, db.Transactions.First().Amount);
        Assert.Equal(account.ID, db.Transactions.First().AccountId);
        Assert.Equal(orderId, db.Transactions.First().OrderId);
    }

    [Fact]
    public async Task GetBalanceAsync_Returns_Sum_Of_Transactions()
    {
        using var db = CreateDbContext();
        var account = new Account(Guid.NewGuid());
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        db.Transactions.Add(new Transaction { Id = Guid.NewGuid(), AccountId = account.ID, Amount = 100m });
        db.Transactions.Add(new Transaction { Id = Guid.NewGuid(), AccountId = account.ID, Amount = -30m });
        await db.SaveChangesAsync();

        var repo = new AccountRepository(db);
        var balance = await repo.GetBalanceAsync(account.ID, CancellationToken.None);

        Assert.Equal(70m, balance);
    }
}
