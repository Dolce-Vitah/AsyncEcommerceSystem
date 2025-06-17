using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Payments.Domain.Interfaces;
using Payments.Domain.Models;
using Payments.UseCases.Commands;
using Xunit;
using MediatR;

public class TopUpAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_TopUp_Account_When_Valid()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var amount = 100m;
        var command = new TopUpAccountCommand(accountId, amount);
        var account = new Account(Guid.NewGuid());
        var repoMock = new Mock<IAccountRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        repoMock.Setup(r => r.GetAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        repoMock.Setup(r => r.AddTransactionAsync(accountId, null, amount, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repoMock.SetupGet(r => r.UnitOfWork).Returns(unitOfWorkMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new TopUpAccountCommandHandler(repoMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Unit.Value, result);
        repoMock.Verify(r => r.AddTransactionAsync(accountId, null, amount, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFoundException_When_Account_Does_Not_Exist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var command = new TopUpAccountCommand(accountId, 50m);
        var repoMock = new Mock<IAccountRepository>();
        repoMock.Setup(r => r.GetAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var handler = new TopUpAccountCommandHandler(repoMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_Throw_ArgumentException_When_Amount_Is_Not_Positive()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new Account(Guid.NewGuid());
        var command = new TopUpAccountCommand(accountId, 0m);
        var repoMock = new Mock<IAccountRepository>();
        repoMock.Setup(r => r.GetAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var handler = new TopUpAccountCommandHandler(repoMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
