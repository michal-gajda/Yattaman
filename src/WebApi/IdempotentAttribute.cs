namespace Yattaman.WebApi;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public sealed class IdempotentAttribute : ActionFilterAttribute
{
    private readonly int cacheTimeInMinutes;
    public IdempotentAttribute(int cacheTimeInMinutes) => this.cacheTimeInMinutes = cacheTimeInMinutes;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        if (request.Headers.TryGetValue("Idempotency-Key", out var key) is false)
        {
            context.Result = new BadRequestObjectResult("Missing Idempotency-Key header.");

            return;
        }

        var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
        var cacheKey = $"Idempotent_{key}";
        var cachedResponse = await cache.GetStringAsync(cacheKey);

        if (cachedResponse is not null)
        {
            var data = JsonSerializer.Deserialize<IdempotentResponse>(cachedResponse)!;
            context.Result = new ObjectResult(data.Value)
            {
                StatusCode = data.StatusCode
            };

            return;
        }

        var executedContext = await next();

        if (executedContext.Result is ObjectResult objectResult)
        {
            var responseToCache = new IdempotentResponse
            {
                Value = objectResult.Value,
                StatusCode = objectResult.StatusCode ?? 200
            };
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(responseToCache), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(this.cacheTimeInMinutes)
            });
        }
    }
}
