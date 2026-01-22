using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FDAAPI.Infra.Services.Notifications
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly FirebaseMessaging _messaging;
        private readonly ILogger<PushNotificationService> _logger;

        public PushNotificationService(ILogger<PushNotificationService> logger, IConfiguration config)
        {
            _logger = logger;

            // Initialize Firebase Admin SDK
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(config["Firebase:ServiceAccountKeyPath"])
                });
            }
            _messaging = FirebaseMessaging.DefaultInstance;
        }

        public async Task<bool> SendPushNotificationAsync(
            string deviceToken,
            string title,
            string body,
            Domain.RelationalDb.Enums.NotificationPriority priority,
            Dictionary<string, string>? data,
            CancellationToken ct)
        {
            try
            {
                // Map NotificationPriority to FCM priority
                var fcmPriority = priority >= Domain.RelationalDb.Enums.NotificationPriority.High
                    ? FirebaseAdmin.Messaging.Priority.High
                    : FirebaseAdmin.Messaging.Priority.Normal;

                var apnsPriority = priority >= Domain.RelationalDb.Enums.NotificationPriority.High ? "10" : "5";

                var message = new Message()
                {
                    Token = deviceToken,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data,
                    Android = new AndroidConfig()
                    {
                        Priority = fcmPriority
                    },
                    Apns = new ApnsConfig()
                    {
                        Headers = new Dictionary<string, string>
                        {
                            ["apns-priority"] = apnsPriority
                        },
                        Aps = new Aps()
                        {
                            Sound = priority >= Domain.RelationalDb.Enums.NotificationPriority.High ? "critical.wav" : "default"
                        }
                    }
                };

                string response = await _messaging.SendAsync(message, ct);
                _logger.LogInformation(
                    "FCM sent with {Priority} priority. MessageId: {MessageId}",
                    fcmPriority, response);
                return true;
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex, "FCM error: {ErrorCode}", ex.MessagingErrorCode);
                return false;
            }
        }
    }
} // ADD THIS CLOSING BRACE