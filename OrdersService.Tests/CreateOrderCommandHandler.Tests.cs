using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Moq;
using Orders.Domain.Interfaces;
using Orders.Domain.Models;
using Orders.UseCases.Commands;
using Xunit;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IOutboxRepository> _outboxRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public CreateOrderCommandHandlerTests()
    {
        _orderRepo.SetupGet(r => r.UnitOfWork).Returns(_unitOfWork.Object);        
    }

    [Fact]
    public async Task Handle_Should_CreateOrder_And_SaveOutbox_And_SaveChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = 100m;
        var handler = new CreateOrderCommandHandler(_orderRepo.Object, _outboxRepo.Object);
        var command = new CreateOrderCommand(userId, amount);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        _orderRepo.Verify(r => r.AddAsync(It.Is<Order>(o => o.UserId == userId && o.Amount == amount), It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepo.Verify(r => r.SaveAsync(
            It.Is<string>(type => type == "OrderCreated"),
            It.Is<string>(payload => payload.Contains(userId.ToString()) && payload.Contains(amount.ToString())),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotEqual(Guid.Empty, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Handle_Should_Throw_When_Amount_Is_Not_Positive(decimal badAmount)
    {
        // Arrange
        var handler = new CreateOrderCommandHandler(_orderRepo.Object, _outboxRepo.Object);
        var command = new CreateOrderCommand(Guid.NewGuid(), badAmount);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }
}
