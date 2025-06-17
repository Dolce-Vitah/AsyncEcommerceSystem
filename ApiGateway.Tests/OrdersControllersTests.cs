using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiGateway.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using Xunit;

public class OrdersControllerTests
{
    private static OrdersController CreateController(HttpResponseMessage responseMessage)
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
        httpClientFactoryMock.Setup(f => f.CreateClient("OrdersService")).Returns(httpClient);

        var controller = new OrdersController(httpClientFactoryMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Test"] = "test";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public async Task CreateOrder_Returns_Status_And_Content()
    {
        var expectedContent = "{\"orderId\":\"123\"}";
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(expectedContent)
        };

        var controller = CreateController(response);

        var result = await controller.CreateOrder(new CreateOrderRequest(Guid.NewGuid(), 100m));

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.Created, objectResult.StatusCode);
        Assert.Equal(expectedContent, objectResult.Value);
    }

    [Fact]
    public async Task GetOrders_Returns_Status_And_Content()
    {
        var expectedContent = "[{\"orderId\":\"123\"}]";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedContent)
        };

        var controller = CreateController(response);

        var result = await controller.GetOrders(Guid.NewGuid());

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
        Assert.Equal(expectedContent, objectResult.Value);
    }

    [Fact]
    public async Task GetOrderStatus_Returns_Status_And_Content()
    {
        var expectedContent = "{\"status\":\"Completed\"}";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedContent)
        };

        var controller = CreateController(response);

        var result = await controller.GetOrderStatus("abc");

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.OK, objectResult.StatusCode);
        Assert.Equal(expectedContent, objectResult.Value);
    }
}
