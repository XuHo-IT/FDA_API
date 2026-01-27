using FDAAPI.Domain.RelationalDb.Enums;
using Microsoft.Extensions.Logging;

namespace FDAAPI.Infra.Services.Notifications
{
    public class MockPushNotificationService : IPushNotificationService
    {
        private readonly ILogger<MockPushNotificationService> _logger;

        public MockPushNotificationService(ILogger<MockPushNotificationService> logger)
        {
            _logger = logger;
        }

        public Task<bool> SendPushNotificationAsync(
            string deviceToken,
            string title,
            string body,
            NotificationPriority priority,
            Dictionary<string, string>? data = null,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "[MOCK] Push notification would be sent to token: {Token}, Title: {Title}, Priority: {Priority}",
                deviceToken.Substring(0, Math.Min(10, deviceToken.Length)) + "...",
                title,
                priority);

            // Simulate success (for testing)
            return Task.FromResult(true);
        }
    }
}