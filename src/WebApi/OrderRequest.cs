namespace Yattaman.WebApi;

public sealed record class OrderRequest
{
    public required string Item { get; init; }
}
