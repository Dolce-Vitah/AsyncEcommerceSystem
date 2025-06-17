using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Payments.Domain.Interfaces;
using Payments.Domain.Models;
using Payments.UseCases.Commands;
using Xunit;

public class CreateAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Account_And_Return_Id_When_Account_Does_Not_Exist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateAccountCommand(userId);
        var repoMock = new Mock<IAccountRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        repoMock.Setup(r => r.GetByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);
        repoMock.Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repoMock.SetupGet(r => r.UnitOfWork).Returns(unitOfWorkMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateAccountCommandHandler(repoMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        repoMock.Verify(r => r.AddAsync(It.Is<Account>(a => a.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Account_Already_Exists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateAccountCommand(userId);
        var existingAccount = new Account(userId);
        var repoMock = new Mock<IAccountRepository>();
        repoMock.Setup(r => r.GetByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        var handler = new CreateAccountCommandHandler(repoMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
