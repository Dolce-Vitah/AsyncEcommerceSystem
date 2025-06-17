using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Payments.Domain.Interfaces;
using Payments.Domain.Models;
using Payments.UseCases.Commands;
using Payments.UseCases.Queries;
using Xunit;

public class GetBalanceQueryHandlerTests
{
    [Fact]
    public async Task Handle_Returns_Balance_When_Account_Exists()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var expectedBalance = 123.45m;
        var repoMock = new Mock<IAccountRepository>();
        var account = new Account(Guid.NewGuid());
        repoMock.Setup(r => r.GetAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        repoMock.Setup(r => r.GetBalanceAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBalance);

        var handler = new GetBalanceQueryHandler(repoMock.Object);
        var query = new GetBalanceQuery(accountId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedBalance, result);
        repoMock.Verify(r => r.GetAsync(accountId, It.IsAny<CancellationToken>()), Times.Once);
        repoMock.Verify(r => r.GetBalanceAsync(accountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_NotFoundException_When_Account_Does_Not_Exist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var repoMock = new Mock<IAccountRepository>();
        repoMock.Setup(r => r.GetAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var handler = new GetBalanceQueryHandler(repoMock.Object);
        var query = new GetBalanceQuery(accountId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(query, CancellationToken.None));
        repoMock.Verify(r => r.GetAsync(accountId, It.IsAny<CancellationToken>()), Times.Once);
        repoMock.Verify(r => r.GetBalanceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
