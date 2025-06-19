using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiGateway.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.Protected;
using Xunit;

public class AccountsControllerTests
{
    private static AccountsController CreateController(HttpResponseMessage responseMessage)
    {
        var httpClientHandlerMock = new Mock<HttpMessageHandler>();
        httpClientHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var httpClient = new HttpClient(httpClientHandlerMock.Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient("PaymentsService")).Returns(httpClient);

        var controller = new AccountsController(httpClientFactoryMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Test"] = "test";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public async Task CreateAccount_Returns_Status_And_Content()
    {
        var expectedContent = "{\"accountId\":\"123\"}";
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(expectedContent)
        };

        var controller = CreateController(response);

        var result = await controller.CreateAccount(new CreateAccountRequest(Guid.NewGuid()));

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.Created, objectResult.StatusCode);
        Assert.Equal(expectedContent, objectResult.Value);
    }

    [Fact]
    public async Task TopUpAccount_Returns_Status_And_Content()
    {
        var expectedContent = "";
        var response = new HttpResponseMessage(HttpStatusCode.NoContent)
        {
            Content = new StringContent(expectedContent)
        };

        var controller = CreateController(response);

        var result = await controller.TopUpAccount("abc", new TopUpAccountRequest(100m));

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.NoContent, objectResult.StatusCode);
        Assert.Equal(expectedContent, objectResult.Value);
    }

    [Fact]
    public async Task GetAccountBalance_Returns_Status_And_Content()
    {
        var expectedContent = "{\"balance\":100.0}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedContent)
        };

        var controller = CreateController(response);

        var result = await controller.GetAccountBalance("abc");

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
        Assert.Equal(expectedContent, objectResult.Value);
    }
}
