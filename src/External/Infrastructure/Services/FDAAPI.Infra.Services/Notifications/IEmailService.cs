namespace FDAAPI.Infra.Services.Notifications
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(
            string toEmail,
            string subject,
            string body,
            CancellationToken ct = default);
    }
}