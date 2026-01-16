using Microsoft.Extensions.Logging;

namespace FDAAPI.Infra.Services.Notifications
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(
            string toEmail,
            string subject,
            string body,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Sending email to {Email}, Subject: {Subject}", toEmail, subject);

                // TODO: Implement SMTP or SendGrid/AWS SES integration
                await Task.Delay(200, ct);

                _logger.LogInformation("Email sent successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }
    }
}