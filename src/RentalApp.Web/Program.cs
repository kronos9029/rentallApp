using System.Security.Claims;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RentalApp.Application.Features.Availability;
using RentalApp.Application.Features.Booking;
using RentalApp.Domain.Enums;
using RentalApp.Domain.Common;
using RentalApp.Infrastructure;
using RentalApp.Infrastructure.Security.Secrets;
using RentalApp.Web.Middleware;
using RentalApp.Web.Contracts.Booking;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.DecryptMarkedValuesFromEnvironment();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "__Host-rentalapp-antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.Name = "__Host-rentalapp-auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(AppRoles.Admin));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute());
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Account");
    options.Conventions.AuthorizePage("/Booking/Hold");
    options.Conventions.AuthorizeFolder("/Checkout");
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
    options.Conventions.AllowAnonymousToPage("/Auth/Register");
    options.Conventions.AllowAnonymousToPage("/Auth/ForgotPassword");
    options.Conventions.AllowAnonymousToPage("/Auth/ResetPassword");
    options.Conventions.AllowAnonymousToPage("/Auth/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Booking/Availability");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCookiePolicy(new CookiePolicyOptions
{
    HttpOnly = HttpOnlyPolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Lax,
    Secure = app.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always
});
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok", service = "web" })).AllowAnonymous();
app.MapGet("/readyz", () => Results.Ok(new { status = "ready", service = "web" })).AllowAnonymous();

app.MapGet("/api/availability", async (
    DateOnly date,
    BookingMode bookingMode,
    byte? slotQty,
    IAvailabilityService availabilityService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await availabilityService.GetAvailabilityAsync(
            new AvailabilityQuery(date, bookingMode, slotQty.GetValueOrDefault(1)),
            cancellationToken);

        return Results.Ok(new { success = true, data = result, errors = Array.Empty<string>() });
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new { success = false, data = (object?)null, errors = new[] { exception.Message } });
    }
}).AllowAnonymous();

app.MapPost("/api/holds", async (
    CreateHoldApiRequest request,
    HttpContext context,
    IHoldService holdService,
    CancellationToken cancellationToken) =>
{
    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(userId))
    {
        return Results.Unauthorized();
    }

    if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var headerValue) || string.IsNullOrWhiteSpace(headerValue))
    {
        return Results.BadRequest(new { success = false, data = (object?)null, errors = new[] { "Idempotency-Key header is required." } });
    }

    try
    {
        var result = await holdService.CreateAsync(
            new CreateHoldRequest(
                userId,
                request.CourtId,
                request.BucketIds,
                request.BookingMode,
                request.BookingMode == BookingMode.Shared ? request.SlotQuantity : (byte)1,
                headerValue.ToString()),
            cancellationToken);

        if (!result.Success)
        {
            return result.Conflict
                ? Results.Conflict(new { success = false, data = (object?)null, errors = new[] { result.Error ?? "Conflict" } })
                : Results.BadRequest(new { success = false, data = (object?)null, errors = new[] { result.Error ?? "Request failed" } });
        }

        return Results.Created($"/booking/hold/{result.Hold!.HoldId}", new { success = true, data = result.Hold, replayed = result.Replayed, errors = Array.Empty<string>() });
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new { success = false, data = (object?)null, errors = new[] { exception.Message } });
    }
}).RequireAuthorization();

app.MapPost("/api/checkout", async (
    CreateCheckoutApiRequest request,
    HttpContext context,
    ICheckoutService checkoutService,
    CancellationToken cancellationToken) =>
{
    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrWhiteSpace(userId))
    {
        return Results.Unauthorized();
    }

    if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var headerValue) || string.IsNullOrWhiteSpace(headerValue))
    {
        return Results.BadRequest(new { success = false, data = (object?)null, errors = new[] { "Idempotency-Key header is required." } });
    }

    try
    {
        var result = await checkoutService.CreateAsync(
            new CreateCheckoutRequest(userId, request.HoldId, headerValue.ToString()),
            cancellationToken);

        if (!result.Success)
        {
            return result.Conflict
                ? Results.Conflict(new { success = false, data = (object?)null, errors = new[] { result.Error ?? "Conflict" } })
                : Results.BadRequest(new { success = false, data = (object?)null, errors = new[] { result.Error ?? "Request failed" } });
        }

        return Results.Created($"/checkout?holdId={request.HoldId}", new { success = true, data = result.Order, replayed = result.Replayed, errors = Array.Empty<string>() });
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new { success = false, data = (object?)null, errors = new[] { exception.Message } });
    }
}).RequireAuthorization();

app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
}).RequireAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();

public partial class Program;
