namespace Yattaman.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    [Idempotent(cacheTimeInMinutes: 60)]
    public IActionResult CreateOrder([FromBody] OrderRequest request)
    {
        var orderId = Guid.NewGuid();

        return CreatedAtAction(nameof(GetOrder), new { id = orderId }, new { Id = orderId, request.Item });
    }

    [HttpGet("{id}")]
    public IActionResult GetOrder(Guid id) => Ok(new { Id = id, Status = "Processed" });
}