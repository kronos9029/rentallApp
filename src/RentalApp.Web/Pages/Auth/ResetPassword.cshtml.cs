using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalApp.Application.Features.Auth;

namespace RentalApp.Web.Pages.Auth;

public sealed class ResetPasswordModel(IUserAuthService authService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public bool IsLinkValid { get; private set; }

    public async Task OnGetAsync(string email, string token)
    {
        Input.Email = email;
        Input.Token = token;
        IsLinkValid = await authService.CanResetPasswordAsync(email, token, HttpContext.RequestAborted);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        IsLinkValid = await authService.CanResetPasswordAsync(Input.Email, Input.Token, HttpContext.RequestAborted);
        if (!ModelState.IsValid || !IsLinkValid)
        {
            if (!IsLinkValid)
            {
                ModelState.AddModelError(string.Empty, "Reset link khong hop le hoac da het han.");
            }

            return Page();
        }

        var result = await authService.ResetPasswordAsync(
            Input.Email,
            Input.Token,
            Input.Password,
            HttpContext.RequestAborted);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Khong the dat lai mat khau.");
            IsLinkValid = false;
            return Page();
        }

        StatusMessage = "Dat lai mat khau thanh cong. Ban co the dang nhap bang mat khau moi.";
        return RedirectToPage("/Auth/Login");
    }

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Mat khau moi")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "Xac nhan mat khau moi")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
