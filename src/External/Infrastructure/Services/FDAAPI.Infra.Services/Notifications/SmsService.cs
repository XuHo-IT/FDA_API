using Microsoft.Extensions.Logging;

namespace FDAAPI.Infra.Services.Notifications
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendSmsAsync(
            string phoneNumber,
            string message,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Sending SMS to {PhoneNumber}", phoneNumber);

                // TODO: Implement Twilio/AWS SNS integration
                await Task.Delay(300, ct);

                _logger.LogInformation("SMS sent successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }
    }
}