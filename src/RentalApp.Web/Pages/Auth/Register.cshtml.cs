using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalApp.Application.Features.Auth;

namespace RentalApp.Web.Pages.Auth;

public sealed class RegisterModel(IUserAuthService authService) : PageModel
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

        var result = await authService.RegisterCustomerAsync(
            Input.Email,
            Input.FullName,
            Input.Password,
            HttpContext.RequestAborted);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Khong the tao tai khoan.");
            return Page();
        }

        StatusMessage = "Tao tai khoan thanh cong. Ban co the dang nhap ngay bay gio.";
        return RedirectToPage("/Auth/Login");
    }

    public sealed class InputModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Ho va ten")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "Xac nhan password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
