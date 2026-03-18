using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalApp.Application.Features.Booking;

namespace RentalApp.Web.Pages.Checkout;

public sealed class IndexModel(ICheckoutService checkoutService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    [Required]
    public string HoldId { get; set; } = string.Empty;

    [BindProperty]
    public CheckoutInputModel Input { get; set; } = new();

    public CheckoutPreview? Preview { get; private set; }

    public CheckoutOrderSummary? Order { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        Preview = await checkoutService.GetPreviewAsync(HoldId, userId, HttpContext.RequestAborted);
        if (Preview is null)
        {
            return NotFound();
        }

        Input.HoldId = HoldId;
        EnsureIdempotencyKey();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            Preview = await checkoutService.GetPreviewAsync(Input.HoldId, userId, HttpContext.RequestAborted);
            EnsureIdempotencyKey();
            return Page();
        }

        var result = await checkoutService.CreateAsync(
            new CreateCheckoutRequest(userId, Input.HoldId, Input.IdempotencyKey),
            HttpContext.RequestAborted);

        if (!result.Success || result.Order is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Khong the tao checkout luc nay.");
            Preview = await checkoutService.GetPreviewAsync(Input.HoldId, userId, HttpContext.RequestAborted);
            EnsureIdempotencyKey();
            return Page();
        }

        Order = result.Order;
        Preview = await checkoutService.GetPreviewAsync(Input.HoldId, userId, HttpContext.RequestAborted);
        Input.HoldId = Input.HoldId;
        Input.IdempotencyKey = Guid.NewGuid().ToString("N");
        return Page();
    }

    private void EnsureIdempotencyKey()
    {
        if (string.IsNullOrWhiteSpace(Input.IdempotencyKey))
        {
            Input.IdempotencyKey = Guid.NewGuid().ToString("N");
        }

        if (string.IsNullOrWhiteSpace(Input.HoldId))
        {
            Input.HoldId = HoldId;
        }
    }

    public sealed class CheckoutInputModel
    {
        [Required]
        public string HoldId { get; set; } = string.Empty;

        [Required]
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
