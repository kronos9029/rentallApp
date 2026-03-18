using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalApp.Web.Security;

namespace RentalApp.Web.Pages.Auth;

public sealed class ForgotPasswordModel(DevelopmentAuthStore authStore) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public string? ResetUrl { get; private set; }

    public DateTimeOffset? ResetUrlExpiresAt { get; private set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = authStore.CreatePasswordResetRequest(Input.Email);
        StatusMessage = "Neu email ton tai, he thong da tao reset flow. Ban co the dung reset link duoi day trong moi truong dev.";

        if (result.UserExists && result.Token is not null && result.ExpiresAt is not null)
        {
            ResetUrl = Url.Page(
                "/Auth/ResetPassword",
                pageHandler: null,
                values: new { email = Input.Email.Trim(), token = result.Token },
                protocol: Request.Scheme);

            ResetUrlExpiresAt = result.ExpiresAt.Value;
        }

        return Page();
    }

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
