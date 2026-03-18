using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalApp.Application.Features.Auth;

namespace RentalApp.Web.Pages.Auth;

public sealed class ForgotPasswordModel(
    IUserAuthService authService,
    IPasswordResetNotificationService passwordResetNotificationService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await authService.CreatePasswordResetRequestAsync(
            Input.Email,
            cancellationToken: HttpContext.RequestAborted);

        if (result.UserExists && result.Token is not null && result.ExpiresAt is not null)
        {
            var resetUrl = Url.Page(
                "/Auth/ResetPassword",
                pageHandler: null,
                values: new { email = Input.Email.Trim(), token = result.Token },
                protocol: Request.Scheme);
            if (string.IsNullOrWhiteSpace(resetUrl))
            {
                ModelState.AddModelError(string.Empty, "Khong the tao reset link hop le.");
                return Page();
            }

            var delivery = await passwordResetNotificationService.SendResetLinkAsync(
                Input.Email.Trim(),
                resetUrl,
                result.ExpiresAt.Value,
                HttpContext.RequestAborted);
            if (!delivery.Success)
            {
                ModelState.AddModelError(string.Empty, delivery.Error ?? "Khong the gui email reset luc nay.");
                return Page();
            }
        }

        StatusMessage = "Neu email ton tai, he thong da gui huong dan reset qua email.";
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
