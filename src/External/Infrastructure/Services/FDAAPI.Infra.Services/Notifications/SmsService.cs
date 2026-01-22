using FDAAPI.Infra.Services.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class SmsService : ISmsService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IConfiguration config, ILogger<SmsService> logger)
    {
        _config = config;
        _logger = logger;

        TwilioClient.Init(_config["Twilio:AccountSid"], _config["Twilio:AuthToken"]);
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken ct)
    {
        try
        {
            var messageResource = await MessageResource.CreateAsync(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(_config["Twilio:PhoneNumber"]),
                body: message
            );

            _logger.LogInformation("SMS sent. SID: {Sid}, Status: {Status}",
                messageResource.Sid, messageResource.Status);

            return messageResource.Status != MessageResource.StatusEnum.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS");
            return false;
        }
    }
}