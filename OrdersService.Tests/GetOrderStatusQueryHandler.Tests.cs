using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Orders.Domain.Interfaces;
using Orders.Domain.Models;
using Orders.Domain.ValueObjects;
using Orders.UseCases.Queries;
using Xunit;

public class GetOrderStatusQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();

    [Fact]
    public async Task Handle_ReturnsOrderStatus_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order(Guid.NewGuid(), 50m);
        order.GetType().GetProperty("ID")?.SetValue(order, orderId); 
        order.GetType().GetProperty("Status")?.SetValue(order, OrderStatus.Paid);

        _orderRepo.Setup(r => r.GetAsync(orderId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(order);

        var handler = new GetOrderStatusQueryHandler(_orderRepo.Object);
        var query = new GetOrderStatusQuery(orderId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(OrderStatus.Paid, result);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepo.Setup(r => r.GetAsync(orderId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((Order)null);

        var handler = new GetOrderStatusQueryHandler(_orderRepo.Object);
        var query = new GetOrderStatusQuery(orderId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }
}
