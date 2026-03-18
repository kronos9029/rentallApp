using RentalApp.Domain.Enums;

namespace RentalApp.Web.Contracts.Booking;

public sealed class CreateHoldApiRequest
{
    public string CourtId { get; set; } = string.Empty;

    public List<string> BucketIds { get; set; } = [];

    public BookingMode BookingMode { get; set; } = BookingMode.Private;

    public byte SlotQuantity { get; set; } = 1;
}
