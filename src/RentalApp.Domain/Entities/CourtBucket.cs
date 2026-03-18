using RentalApp.Domain.Enums;

namespace RentalApp.Domain.Entities;

public sealed class CourtBucket
{
    public string CourtId { get; set; } = string.Empty;

    public string BucketId { get; set; } = string.Empty;

    public CourtBucketMode Mode { get; set; } = CourtBucketMode.None;

    public byte SharedCapacity { get; set; } = 8;

    public byte SharedReserved { get; set; }

    public string? PrivateHoldId { get; set; }

    public uint LockVersion { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Court Court { get; set; } = null!;

    public TimeBucket TimeBucket { get; set; } = null!;
}
