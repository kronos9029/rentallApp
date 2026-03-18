using System.Net;

namespace RentalApp.Infrastructure.Communication.Email;

internal static class PasswordResetEmailTemplate
{
    public static string CreateSubject()
    {
        return "RentalApp - Password reset";
    }

    public static string CreatePlainText(string resetUrl, DateTimeOffset expiresAt)
    {
        return
$""""
Ban da yeu cau dat lai mat khau cho RentalApp.

Reset link co hieu luc den: {expiresAt.LocalDateTime:yyyy-MM-dd HH:mm}

Mo link sau de dat lai mat khau:
{resetUrl}

Neu ban khong thuc hien yeu cau nay, hay bo qua email nay.
"""";
    }

    public static string CreateHtml(string resetUrl, DateTimeOffset expiresAt)
    {
        var escapedUrl = WebUtility.HtmlEncode(resetUrl);
        var escapedExpiry = WebUtility.HtmlEncode(expiresAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm"));

        return
$""""
<html>
  <body style="font-family:Segoe UI,Arial,sans-serif;color:#1f2937;line-height:1.5;">
    <h2 style="margin-bottom:12px;">Dat lai mat khau RentalApp</h2>
    <p>Ban da yeu cau dat lai mat khau cho RentalApp.</p>
    <p>Reset link co hieu luc den: <strong>{escapedExpiry}</strong></p>
    <p>
      <a href="{escapedUrl}" style="display:inline-block;padding:10px 16px;background:#0d6efd;color:#ffffff;text-decoration:none;border-radius:6px;">
        Dat lai mat khau
      </a>
    </p>
    <p>Neu nut khong mo duoc, hay copy link sau vao trinh duyet:</p>
    <p><a href="{escapedUrl}">{escapedUrl}</a></p>
    <p>Neu ban khong thuc hien yeu cau nay, hay bo qua email nay.</p>
  </body>
</html>
"""";
    }
}
