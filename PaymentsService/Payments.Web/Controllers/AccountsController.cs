using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payments.UseCases.Commands;
using Payments.UseCases.Queries;

namespace Payments.Web.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AccountsController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountCommand cmd)
        {
            try
            {
                var id = await _mediator.Send(cmd);
                return CreatedAtAction(nameof(Balance), new { accountId = id }, new { accountId = id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{accountId}/topup")]
        public async Task<IActionResult> TopUp(Guid accountId, [FromBody] TopUpAccountRequest request)
        {
            try
            {
                await _mediator.Send(new TopUpAccountCommand(accountId, request.Amount));
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("{accountId}/balance")]
        public async Task<IActionResult> Balance(Guid accountId)
        {
            try
            {
                var bal = await _mediator.Send(new GetBalanceQuery(accountId));
                return bal is null
                    ? NotFound()
                    : Ok(new { balance = bal });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }

    public record TopUpAccountRequest(decimal Amount);
}
