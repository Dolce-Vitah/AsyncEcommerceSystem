using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Models;
using Orders.Domain.ValueObjects;
using Orders.Infrastructure.Database;
using Orders.Infrastructure.Repositories;
using Xunit;

public class OrderRepositoryTests
{
    private OrdersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new OrdersDbContext(options);
    }

    [Fact]
    public async Task AddAsync_AddsOrderToDb()
    {
        using var db = CreateDbContext();
        var repo = new OrderRepository(db);

        var order = new Order(Guid.NewGuid(), 123m);
        await repo.AddAsync(order, CancellationToken.None);
        await db.SaveChangesAsync();

        var found = await db.Orders.FindAsync(order.ID);
        Assert.NotNull(found);
        Assert.Equal(order.UserId, found.UserId);
        Assert.Equal(order.Amount, found.Amount);
        Assert.Equal(OrderStatus.Pending, found.Status);
    }

    [Fact]
    public async Task GetAsync_ReturnsOrder_WhenExists()
    {
        using var db = CreateDbContext();
        var order = new Order(Guid.NewGuid(), 50m);
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var repo = new OrderRepository(db);
        var result = await repo.GetAsync(order.ID, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(order.ID, result.ID);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenNotExists()
    {
        using var db = CreateDbContext();
        var repo = new OrderRepository(db);

        var result = await repo.GetAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ListByUserAsync_ReturnsOrdersForUser()
    {
        using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        db.Orders.Add(new Order(userId, 10m));
        db.Orders.Add(new Order(userId, 20m));
        db.Orders.Add(new Order(Guid.NewGuid(), 30m));
        await db.SaveChangesAsync();

        var repo = new OrderRepository(db);
        var result = await repo.ListByUserAsync(userId, CancellationToken.None);

        Assert.Equal(2, result.Count());
        Assert.All(result, o => Assert.Equal(userId, o.UserId));
    }
}
