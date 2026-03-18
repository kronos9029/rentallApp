using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RentalApp.Domain.Common;
using RentalApp.Web.Middleware;
using RentalApp.Web.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DevelopmentAuthOptions>(
    builder.Configuration.GetSection(DevelopmentAuthOptions.SectionName));

builder.Services.AddSingleton<DevelopmentAuthStore>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(AppRoles.Admin));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Account");
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
    options.Conventions.AllowAnonymousToPage("/Auth/Register");
    options.Conventions.AllowAnonymousToPage("/Auth/ForgotPassword");
    options.Conventions.AllowAnonymousToPage("/Auth/ResetPassword");
    options.Conventions.AllowAnonymousToPage("/Auth/AccessDenied");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok", service = "web" })).AllowAnonymous();
app.MapGet("/readyz", () => Results.Ok(new { status = "ready", service = "web" })).AllowAnonymous();

app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
}).RequireAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
