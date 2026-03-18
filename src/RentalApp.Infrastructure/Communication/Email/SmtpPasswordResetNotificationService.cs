using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RentalApp.Application.Features.Auth;

namespace RentalApp.Infrastructure.Communication.Email;

public sealed class SmtpPasswordResetNotificationService(
    IOptions<SmtpOptions> options,
    ILogger<SmtpPasswordResetNotificationService> logger) : IPasswordResetNotificationService
{
    private readonly SmtpOptions _options = options.Value;
    private readonly ILogger<SmtpPasswordResetNotificationService> _logger = logger;

    public async Task<OperationResult> SendResetLinkAsync(
        string email,
        string resetUrl,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogError("SMTP configuration is incomplete. Forgot-password email dispatch is unavailable.");
            return new OperationResult(false, "Email reset tam thoi chua duoc cau hinh day du.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromDisplayName),
            Subject = PasswordResetEmailTemplate.CreateSubject(),
            Body = PasswordResetEmailTemplate.CreateHtml(resetUrl, expiresAt),
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(email));
        message.AlternateViews.Add(
            AlternateView.CreateAlternateViewFromString(
                PasswordResetEmailTemplate.CreatePlainText(resetUrl, expiresAt),
                null,
                "text/plain"));

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Password reset email dispatched to {Email}.", email);
            return new OperationResult(true, null);
        }
        catch (SmtpException exception)
        {
            _logger.LogError(exception, "SMTP dispatch failed for password reset email to {Email}.", email);
            return new OperationResult(false, "Khong the gui email reset luc nay.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected email dispatch failure for {Email}.", email);
            return new OperationResult(false, "Khong the gui email reset luc nay.");
        }
    }
}
