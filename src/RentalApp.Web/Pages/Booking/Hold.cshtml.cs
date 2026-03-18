using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalApp.Application.Features.Booking;

namespace RentalApp.Web.Pages.Booking;

public sealed class HoldModel(IHoldService holdService) : PageModel
{
    public HoldSummary? Hold { get; private set; }

    public async Task<IActionResult> OnGetAsync(string holdId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        Hold = await holdService.GetSummaryAsync(holdId, userId, HttpContext.RequestAborted);
        if (Hold is null)
        {
            return NotFound();
        }

        return Page();
    }
}
