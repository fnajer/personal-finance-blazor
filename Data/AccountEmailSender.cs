using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;

namespace FinanceTracker.Data;

public interface IAccountEmailSender
{
    Task SendConfirmationLinkAsync(string email, string confirmationLink, CancellationToken cancellationToken = default);
    Task SendPasswordResetLinkAsync(string email, string resetLink, CancellationToken cancellationToken = default);
}

public sealed class AccountEmailSender(
    IOptions<EmailOptions> options,
    IWebHostEnvironment environment,
    ILogger<AccountEmailSender> logger) : IAccountEmailSender
{
    private readonly EmailOptions options = options.Value;

    public Task SendConfirmationLinkAsync(string email, string confirmationLink, CancellationToken cancellationToken = default) =>
        SendAsync(email, "Подтвердите email — Мои финансы", "Подтверждение email",
            "Подтвердите адрес, чтобы войти в аккаунт.", "Подтвердить email", confirmationLink, cancellationToken);

    public Task SendPasswordResetLinkAsync(string email, string resetLink, CancellationToken cancellationToken = default) =>
        SendAsync(email, "Восстановление пароля — Мои финансы", "Восстановление пароля",
            "Мы получили запрос на смену пароля. Если это были не вы, просто проигнорируйте письмо.",
            "Задать новый пароль", resetLink, cancellationToken);

    private async Task SendAsync(string recipient, string subject, string heading, string message,
        string buttonText, string link, CancellationToken cancellationToken)
    {
        if (!options.IsConfigured)
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Отправка email не настроена. Заполните секцию Email в конфигурации приложения.");
            }

            logger.LogWarning("SMTP не настроен. Письмо для {Recipient} ({Subject}): {Link}", recipient, subject, link);
            return;
        }

        var encoder = HtmlEncoder.Default;
        var html = $$"""
            <!doctype html>
            <html lang="ru">
            <body style="margin:0;padding:32px;background:#f4f7f3;font-family:Arial,sans-serif;color:#17211b">
              <div style="max-width:560px;margin:auto;padding:32px;background:#fff;border:1px solid #dce5de;border-radius:16px">
                <h1 style="margin:0 0 14px;font-size:26px">{{encoder.Encode(heading)}}</h1>
                <p style="margin:0 0 24px;line-height:1.6;color:#68766d">{{encoder.Encode(message)}}</p>
                <a href="{{encoder.Encode(link)}}" style="display:inline-block;padding:13px 20px;border-radius:9px;background:#176b45;color:#fff;text-decoration:none;font-weight:bold">{{encoder.Encode(buttonText)}}</a>
                <p style="margin:24px 0 0;font-size:12px;line-height:1.5;color:#68766d">Если кнопка не работает, скопируйте ссылку:<br>{{encoder.Encode(link)}}</p>
              </div>
            </body>
            </html>
            """;

        using var mail = new MailMessage
        {
            From = new MailAddress(options.FromAddress, options.FromName),
            Subject = subject,
            Body = html,
            IsBodyHtml = true
        };
        mail.To.Add(new MailAddress(recipient));

        using var client = new SmtpClient(options.Host, options.Port) { EnableSsl = options.EnableSsl };
        if (!string.IsNullOrWhiteSpace(options.Username))
        {
            client.Credentials = new NetworkCredential(options.Username, options.Password);
        }

        await client.SendMailAsync(mail, cancellationToken);
    }
}
