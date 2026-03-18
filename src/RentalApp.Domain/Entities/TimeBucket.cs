namespace RentalApp.Domain.Entities;

public sealed class TimeBucket
{
    public string BucketId { get; set; } = string.Empty;

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public short DurationMin { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<CourtBucket> CourtBuckets { get; set; } = [];

    public ICollection<HoldItem> HoldItems { get; set; } = [];

    public ICollection<BookingItem> BookingItems { get; set; } = [];
}
