using MediatR;
using Microsoft.AspNetCore.Mvc;
using Orders.Domain.Models;
using Orders.UseCases.Commands;
using Orders.UseCases.Queries;

namespace Orders.Web.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _med;

        public OrdersController(IMediator med) => _med = med;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand cmd)
        {
            try
            {
                var id = await _med.Send(cmd);
                return CreatedAtAction(nameof(Status), new { orderId = id }, new { orderId = id });
            } catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }           
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> List([FromQuery] Guid userId)
            => await _med.Send(new GetOrderListQuery(userId));

        [HttpGet("{orderId}/status")]
        public async Task<IActionResult> Status(Guid orderId)
        {
            try
            {
                var st = await _med.Send(new GetOrderStatusQuery(orderId));
                return st is null ? NotFound() : Ok(new { status = st });
            } catch (NotFoundException ex)
            {
                return BadRequest(new { error = ex.Message });
            }            
        }
    }
}
