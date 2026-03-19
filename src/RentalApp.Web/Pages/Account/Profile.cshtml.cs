using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalApp.Application.Features.Auth;
using RentalApp.Web.Security;

namespace RentalApp.Web.Pages.Account;

[Authorize]
public sealed class ProfileModel(IUserAuthService authService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<string> Roles { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await LoadCurrentUserAsync(hydrateInput: true);
        return user is null ? RedirectToPage("/Auth/Login") : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentUser = await LoadCurrentUserAsync(hydrateInput: false);
        if (currentUser is null)
        {
            return RedirectToPage("/Auth/Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await authService.UpdateProfileAsync(userId, Input.FullName, HttpContext.RequestAborted);
        if (!result.Success || result.User is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Khong the cap nhat profile.");
            return Page();
        }

        var existingTicket = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            DbAuthenticatedUserClaimsPrincipalFactory.Create(result.User),
            existingTicket.Properties);

        StatusMessage = "Cap nhat profile thanh cong.";
        return RedirectToPage();
    }

    private async Task<AuthenticatedUser?> LoadCurrentUserAsync(bool hydrateInput)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user = await authService.GetProfileAsync(userId, HttpContext.RequestAborted);
        if (user is null)
        {
            return null;
        }

        if (hydrateInput)
        {
            Input.FullName = user.FullName;
            Input.Email = user.Email;
        }

        Roles = user.Roles;
        return user;
    }

    public sealed class InputModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Ho va ten")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
