using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// Alias để tránh conflict
using DomainNotificationPriority = FDAAPI.Domain.RelationalDb.Enums.NotificationPriority;

namespace FDAAPI.Infra.Services.Notifications
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly FirebaseMessaging? _messaging;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly bool _isEnabled;

        public PushNotificationService(ILogger<PushNotificationService> logger, IConfiguration configuration)
        {
            _logger = logger;

            try
            {
                var firebaseSection = configuration.GetSection("Firebase");
                var projectId = firebaseSection["ProjectId"];
                var privateKey = firebaseSection["PrivateKey"];
                var clientEmail = firebaseSection["ClientEmail"];

                if (string.IsNullOrEmpty(projectId) ||
                    string.IsNullOrEmpty(privateKey) ||
                    string.IsNullOrEmpty(clientEmail))
                {
                    _logger.LogWarning("Firebase config incomplete in appsettings. Push notifications disabled.");
                    _isEnabled = false;
                    return;
                }

                // Tạo JSON credentials từ appsettings
                var credentialJson = new
                {
                    type = firebaseSection["Type"] ?? "service_account",
                    project_id = projectId ?? "your-project-id",
                    private_key_id = firebaseSection["PrivateKeyId"] ?? "abc123def456",
                    private_key = privateKey ?? "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBg...\n-----END PRIVATE KEY-----\n",
                    client_email = clientEmail ?? "firebase-adminsdk-xxxxx@your-project.iam.gserviceaccount.com",
                    client_id = firebaseSection["ClientId"] ?? "123456789012345678901",
                    auth_uri = firebaseSection["AuthUri"] ?? "https://accounts.google.com/o/oauth2/auth",
                    token_uri = firebaseSection["TokenUri"] ?? "https://oauth2.googleapis.com/token",
                    auth_provider_x509_cert_url = firebaseSection["AuthProviderCertUrl"] ?? "https://www.googleapis.com/oauth2/v1/certs",
                    client_x509_cert_url = firebaseSection["ClientCertUrl"] ?? "https://www.googleapis.com/robot/v1/metadata/x509/..."
                };

                var credentialJsonString = JsonConvert.SerializeObject(credentialJson);
                var credential = GoogleCredential.FromJson(credentialJsonString);

                //var credentialPath = Path.Combine(AppContext.BaseDirectory, "firebase-adminsdk.json");
                //var credential = GoogleCredential.FromFile(credentialPath);

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential,
                        ProjectId = projectId
                    });
                }

                _messaging = FirebaseMessaging.DefaultInstance;
                _isEnabled = true;

                _logger.LogInformation("✅ Firebase Push Notification enabled for project: {ProjectId}", projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize Firebase. Push notifications disabled.");
                _isEnabled = false;
            }
        }

        public async Task<bool> SendPushNotificationAsync(
            string deviceToken,
            string title,
            string body,
            DomainNotificationPriority priority,
            Dictionary<string, string>? data = null,
            CancellationToken ct = default)
        {
            if (!_isEnabled || _messaging == null)
            {
                _logger.LogWarning("Push notification skipped (Firebase disabled)");
                return true;
            }

            try
            {
                var fcmPriority = priority >= DomainNotificationPriority.High
                    ? Priority.High
                    : Priority.Normal;

                var apnsPriority = priority >= DomainNotificationPriority.High ? "10" : "5";

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
                        }
                    }
                };

                var response = await _messaging.SendAsync(message, ct);

                _logger.LogInformation(
                    "✅ Push notification sent. Token: {Token}, Title: {Title}, Response: {Response}",
                    deviceToken.Substring(0, Math.Min(10, deviceToken.Length)) + "...",
                    title,
                    response);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send push notification");
                return false;
            }
        }
    }
}