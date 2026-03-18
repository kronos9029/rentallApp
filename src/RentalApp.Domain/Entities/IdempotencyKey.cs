namespace RentalApp.Domain.Entities;

public sealed class IdempotencyKey
{
    public string IdempotencyId { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string IdempotencyKeyValue { get; set; } = string.Empty;

    public string RequestHash { get; set; } = string.Empty;

    public int ResponseStatus { get; set; }

    public string ResponseSnapshot { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
