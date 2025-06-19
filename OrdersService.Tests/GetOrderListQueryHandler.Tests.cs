using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Orders.Domain.Interfaces;
using Orders.Domain.Models;
using Orders.UseCases.Queries;
using Xunit;

public class GetOrderListQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();

    [Fact]
    public async Task Handle_ReturnsOrders_ForGivenUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new Order(userId, 10m),
            new Order(userId, 20m)
        };
        _orderRepo.Setup(r => r.ListByUserAsync(userId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(orders);

        var handler = new GetOrderListQueryHandler(_orderRepo.Object);
        var query = new GetOrderListQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, o => Assert.Equal(userId, o.UserId));
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _orderRepo.Setup(r => r.ListByUserAsync(userId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<Order>());

        var handler = new GetOrderListQueryHandler(_orderRepo.Object);
        var query = new GetOrderListQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
