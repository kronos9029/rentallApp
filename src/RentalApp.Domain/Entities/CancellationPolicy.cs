using RentalApp.Domain.Enums;

namespace RentalApp.Domain.Entities;

public sealed class CancellationPolicy
{
    public string PolicyId { get; set; } = string.Empty;

    public string PolicyName { get; set; } = string.Empty;

    public int MinHoursBeforeStart { get; set; } = 2;

    public RefundStrategy RefundStrategy { get; set; } = RefundStrategy.Full;

    public decimal? RefundPercent { get; set; }

    public decimal CancelFeePercent { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; } = [];
}
