namespace Vargshala.Application.Abstractions.Email;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? from = null, CancellationToken cancellationToken = default);
    Task<bool> SendEmailAsync(EmailMessageRequest request, CancellationToken cancellationToken = default);
}
