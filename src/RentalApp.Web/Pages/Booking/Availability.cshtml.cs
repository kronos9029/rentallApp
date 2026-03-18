using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalApp.Application.Features.Availability;
using RentalApp.Application.Features.Booking;
using RentalApp.Domain.Enums;

namespace RentalApp.Web.Pages.Booking;

public sealed class AvailabilityModel(
    IAvailabilityService availabilityService,
    IHoldService holdService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public FilterInputModel Filter { get; set; } = new()
    {
        BookingDate = DateOnly.FromDateTime(DateTime.UtcNow),
        BookingMode = BookingMode.Private,
        SlotQuantity = 1
    };

    [BindProperty]
    public CreateHoldInputModel HoldInput { get; set; } = new();

    public AvailabilityResult? Result { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAvailabilityAsync();
    }

    public async Task<IActionResult> OnPostCreateHoldAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = Url.Page("/Booking/Availability", new
            {
                bookingDate = Filter.BookingDate.ToString("yyyy-MM-dd"),
                bookingMode = Filter.BookingMode,
                slotQuantity = Filter.SlotQuantity
            }) });
        }

        if (string.IsNullOrWhiteSpace(HoldInput.SelectedCourtId))
        {
            ModelState.AddModelError($"{nameof(HoldInput)}.{nameof(HoldInput.SelectedCourtId)}", "Ban can chon mot san.");
        }

        if (HoldInput.SelectedBucketIds.Count == 0)
        {
            ModelState.AddModelError($"{nameof(HoldInput)}.{nameof(HoldInput.SelectedBucketIds)}", "Ban can chon it nhat mot khung gio.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAvailabilityAsync();
            EnsureIdempotencyKey();
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var result = await holdService.CreateAsync(
            new CreateHoldRequest(
                userId,
                HoldInput.SelectedCourtId,
                HoldInput.SelectedBucketIds,
                Filter.BookingMode,
                Filter.BookingMode == BookingMode.Shared ? HoldInput.SlotQuantity : (byte)1,
                HoldInput.IdempotencyKey),
            HttpContext.RequestAborted);

        if (!result.Success || result.Hold is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Khong the tao hold luc nay.");
            await LoadAvailabilityAsync();
            EnsureIdempotencyKey();
            return Page();
        }

        return RedirectToPage("/Booking/Hold", new { holdId = result.Hold.HoldId });
    }

    private async Task LoadAvailabilityAsync()
    {
        Result = await availabilityService.GetAvailabilityAsync(
            new AvailabilityQuery(Filter.BookingDate, Filter.BookingMode, Filter.SlotQuantity),
            HttpContext.RequestAborted);

        HoldInput.SlotQuantity = Filter.BookingMode == BookingMode.Shared ? Filter.SlotQuantity : (byte)1;
        EnsureIdempotencyKey();
    }

    private void EnsureIdempotencyKey()
    {
        if (string.IsNullOrWhiteSpace(HoldInput.IdempotencyKey))
        {
            HoldInput.IdempotencyKey = Guid.NewGuid().ToString("N");
        }
    }

    public sealed class FilterInputModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Ngay booking")]
        public DateOnly BookingDate { get; set; }

        [Display(Name = "Loai booking")]
        public BookingMode BookingMode { get; set; } = BookingMode.Private;

        [Range(1, 8)]
        [Display(Name = "So slot shared")]
        public byte SlotQuantity { get; set; } = 1;
    }

    public sealed class CreateHoldInputModel
    {
        [Display(Name = "San")]
        public string SelectedCourtId { get; set; } = string.Empty;

        public List<string> SelectedBucketIds { get; set; } = [];

        [Range(1, 8)]
        public byte SlotQuantity { get; set; } = 1;

        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
