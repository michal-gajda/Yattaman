namespace Yattaman.WebApi;

public sealed record class IdempotentResponse
{
    public required object? Value { get; init; }
    public required int StatusCode { get; init; }
}
