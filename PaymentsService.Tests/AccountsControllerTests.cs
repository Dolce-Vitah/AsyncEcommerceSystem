using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Payments.UseCases.Commands;
using Payments.UseCases.Queries;
using Payments.Web.Controllers;
using Xunit;

public class AccountsControllerTests
{
    [Fact]
    public async Task Create_Returns_CreatedAtAction_On_Success()
    {
        var mediator = new Mock<IMediator>();
        var cmd = new CreateAccountCommand(Guid.NewGuid());
        var accountId = Guid.NewGuid();
        mediator.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>())).ReturnsAsync(accountId);

        var controller = new AccountsController(mediator.Object);

        var result = await controller.Create(cmd);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(AccountsController.Balance), created.ActionName);
        Assert.NotNull(created.Value);
        var value = created.Value.GetType().GetProperty("accountId")?.GetValue(created.Value, null);
        Assert.Equal(accountId, value);
    }

    [Fact]
    public async Task Create_Returns_BadRequest_On_Exception()
    {
        var mediator = new Mock<IMediator>();
        var cmd = new CreateAccountCommand(Guid.NewGuid());
        mediator.Setup(m => m.Send(cmd, It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));

        var controller = new AccountsController(mediator.Object);

        var result = await controller.Create(cmd);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("fail", badRequest.Value.ToString());
    }

    [Fact]
    public async Task TopUp_Returns_NoContent_On_Success()
    {
        var mediator = new Mock<IMediator>();
        var accountId = Guid.NewGuid();
        var request = new TopUpAccountRequest(100m);
        mediator.Setup(m => m.Send(It.Is<TopUpAccountCommand>(c => c.AccountId == accountId && c.Amount == 100m), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var controller = new AccountsController(mediator.Object);

        var result = await controller.TopUp(accountId, request);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task TopUp_Returns_NotFound_On_Exception()
    {
        var mediator = new Mock<IMediator>();
        var accountId = Guid.NewGuid();
        var request = new TopUpAccountRequest(100m);
        mediator.Setup(m => m.Send(It.IsAny<TopUpAccountCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("not found"));

        var controller = new AccountsController(mediator.Object);

        var result = await controller.TopUp(accountId, request);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFound.Value.ToString());
    }

    [Fact]
    public async Task Balance_Returns_Ok_With_Balance()
    {
        var mediator = new Mock<IMediator>();
        var accountId = Guid.NewGuid();
        mediator.Setup(m => m.Send(It.Is<GetBalanceQuery>(q => q.AccountId == accountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(123.45m);

        var controller = new AccountsController(mediator.Object);

        var result = await controller.Balance(accountId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
        var value = ok.Value.GetType().GetProperty("balance")?.GetValue(ok.Value, null);
        Assert.Equal(123.45m, value);
    }

    [Fact]
    public async Task Balance_Returns_NotFound_When_Balance_Is_Null()
    {
        var mediator = new Mock<IMediator>();
        var accountId = Guid.NewGuid();
        mediator.Setup(m => m.Send(It.IsAny<GetBalanceQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((decimal?)null);

        var controller = new AccountsController(mediator.Object);

        var result = await controller.Balance(accountId);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Balance_Returns_NotFound_On_Exception()
    {
        var mediator = new Mock<IMediator>();
        var accountId = Guid.NewGuid();
        mediator.Setup(m => m.Send(It.IsAny<GetBalanceQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("fail"));

        var controller = new AccountsController(mediator.Object);

        var result = await controller.Balance(accountId);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("fail", notFound.Value.ToString());
    }
}
