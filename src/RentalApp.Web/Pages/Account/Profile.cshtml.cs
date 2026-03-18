using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalApp.Web.Security;

namespace RentalApp.Web.Pages.Account;

[Authorize]
public sealed class ProfileModel(DevelopmentAuthStore authStore) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<string> Roles { get; private set; } = [];

    public void OnGet()
    {
        LoadCurrentUser();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Input.Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        Roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var result = authStore.UpdateProfile(email, Input.FullName);
        if (!result.Success || result.Principal is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Khong the cap nhat profile.");
            return Page();
        }

        var existingTicket = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            result.Principal,
            existingTicket.Properties);

        StatusMessage = "Cap nhat profile thanh cong.";
        return RedirectToPage();
    }

    private void LoadCurrentUser()
    {
        Input.FullName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        Input.Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        Roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();
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
