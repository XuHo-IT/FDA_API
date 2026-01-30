using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.App.Common.Services;
using Microsoft.Extensions.Logging;

namespace FDAAPI.Infra.Services.Notifications
{
    public class OtpSender : IOtpSender
    {
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OtpSender> _logger;

        public OtpSender(
            ISmsService smsService,
            IEmailService emailService,
            ILogger<OtpSender> logger)
        {
            _smsService = smsService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> SendOtpAsync(
            string identifier,
            string identifierType,
            string otpCode,
            CancellationToken ct = default)
        {
            try
            {
                if (identifierType == "phone")
                {
                    var message = $"Your FDA verification code is: {otpCode}. Valid for 5 minutes.";
                    return await _smsService.SendSmsAsync(identifier, message, ct);
                }
                else
                {
                    var subject = "FDA - Your Verification Code";
                    var body = $@"
                        <h2>Your Verification Code</h2>
                        <p>Your OTP code is: <strong>{otpCode}</strong></p>
                        <p>This code is valid for 5 minutes.</p>
                        <p>If you did not request this, please ignore this email.</p>
                        <br/>
                        <p>- FDA Team</p>";
                    return await _emailService.SendEmailAsync(identifier, subject, body, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP to {Identifier} via {Type}", identifier, identifierType);
                return false;
            }
        }
    }
}
