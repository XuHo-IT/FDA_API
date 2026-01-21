using FDAAPI.Infra.Services.Notifications;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
        Dictionary<string, string>? data,
        CancellationToken ct)
    {
        try
        {
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
                    Priority = Priority.High
                },
                Apns = new ApnsConfig()
                {
                    Aps = new Aps()
                    {
                        Sound = "default"
                    }
                }
            };

            string response = await _messaging.SendAsync(message, ct);
            _logger.LogInformation("FCM sent successfully. MessageId: {MessageId}", response);
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "FCM error: {ErrorCode}", ex.MessagingErrorCode);
            return false;
        }
    }
}