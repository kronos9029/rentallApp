using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RentalApp.Web.Areas.Admin.Pages;

[Authorize(Roles = "Admin")]
public sealed class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
