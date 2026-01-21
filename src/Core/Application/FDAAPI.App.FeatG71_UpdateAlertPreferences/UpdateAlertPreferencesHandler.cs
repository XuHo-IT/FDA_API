using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FDAAPI.App.FeatG71_UpdateAlertPreferences
{
    public class UpdateAlertPreferencesHandler : IRequestHandler<UpdateAlertPreferencesRequest, UpdateAlertPreferencesResponse>
    {
        private readonly IUserAlertSubscriptionRepository _subscriptionRepo;
        private readonly ILogger<UpdateAlertPreferencesHandler> _logger;

        public UpdateAlertPreferencesHandler(
            IUserAlertSubscriptionRepository subscriptionRepo,
            ILogger<UpdateAlertPreferencesHandler> logger)
        {
            _subscriptionRepo = subscriptionRepo;
            _logger = logger;
        }

        public async Task<UpdateAlertPreferencesResponse> Handle(
            UpdateAlertPreferencesRequest request,
            CancellationToken ct)
        {
            try
            {
                // 1. Get existing subscription
                var subscription = await _subscriptionRepo.GetByIdAsync(request.SubscriptionId, ct);

                if (subscription == null)
                {
                    return new UpdateAlertPreferencesResponse
                    {
                        Success = false,
                        Message = "Subscription not found"
                    };
                }

                // 2. Check ownership
                if (subscription.UserId != request.UserId)
                {
                    return new UpdateAlertPreferencesResponse
                    {
                        Success = false,
                        Message = "You can only update your own alert preferences"
                    };
                }

                // 3. Update only provided fields (partial update)
                if (request.MinSeverity != null)
                {
                    subscription.MinSeverity = request.MinSeverity;
                }

                if (request.EnablePush.HasValue)
                {
                    subscription.EnablePush = request.EnablePush.Value;
                }

                if (request.EnableEmail.HasValue)
                {
                    subscription.EnableEmail = request.EnableEmail.Value;
                }

                if (request.EnableSms.HasValue)
                {
                    subscription.EnableSms = request.EnableSms.Value;
                }

                if (request.QuietHoursStart.HasValue)
                {
                    subscription.QuietHoursStart = request.QuietHoursStart;
                }

                if (request.QuietHoursEnd.HasValue)
                {
                    subscription.QuietHoursEnd = request.QuietHoursEnd;
                }

                // 4. Validate: At least one channel must be enabled
                if (!subscription.EnablePush && !subscription.EnableEmail && !subscription.EnableSms)
                {
                    return new UpdateAlertPreferencesResponse
                    {
                        Success = false,
                        Message = "At least one notification channel must be enabled"
                    };
                }

                // 5. Update audit fields
                subscription.UpdatedBy = request.UserId;
                subscription.UpdatedAt = DateTime.UtcNow;

                // 6. Save to database
                var updated = await _subscriptionRepo.UpdateAsync(subscription, ct);

                if (!updated)
                {
                    return new UpdateAlertPreferencesResponse
                    {
                        Success = false,
                        Message = "Failed to update alert preferences"
                    };
                }

                _logger.LogInformation(
                    "User {UserId} updated alert preferences for subscription {SubscriptionId}",
                    request.UserId, request.SubscriptionId);

                return new UpdateAlertPreferencesResponse
                {
                    Success = true,
                    Message = "Alert preferences updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating alert preferences");
                return new UpdateAlertPreferencesResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}