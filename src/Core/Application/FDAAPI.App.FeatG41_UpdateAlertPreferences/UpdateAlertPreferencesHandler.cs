using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FDAAPI.App.FeatG41_UpdateAlertPreferences
{
    public class UpdateAlertPreferencesHandler : IRequestHandler<UpdateAlertPreferencesRequest, UpdateAlertPreferencesResponse>
    {
        private readonly IUserAlertSubscriptionRepository _subscriptionRepo;
        private readonly IAreaRepository _areaRepo;
        private readonly ILogger<UpdateAlertPreferencesHandler> _logger;

        public UpdateAlertPreferencesHandler(
            IUserAlertSubscriptionRepository subscriptionRepo,
            IAreaRepository areaRepo,
            ILogger<UpdateAlertPreferencesHandler> logger)
        {
            _subscriptionRepo = subscriptionRepo;
            _areaRepo = areaRepo;
            _logger = logger;
        }

        public async Task<UpdateAlertPreferencesResponse> Handle(
            UpdateAlertPreferencesRequest request,
            CancellationToken ct)
        {
            try
            {
                var subscriptions = await _subscriptionRepo.GetByAreaIdAsync(request.AreaId, ct);
                var subscription = subscriptions.FirstOrDefault();

                if (subscription == null)
                {
                    return new UpdateAlertPreferencesResponse
                    {
                        Success = false,
                        Message = "No subscription found for this area. Please create an area first."
                    };
                }

                // Authorization: Check if subscription belongs to user
                if (subscription.UserId != request.UserId)
                {
                    return new UpdateAlertPreferencesResponse
                    {
                        Success = false,
                        Message = "Unauthorized: This area does not belong to you"
                    };
                }

                // Update only provided fields
                if (request.MinSeverity != null)
                    subscription.MinSeverity = request.MinSeverity;
                if (request.EnablePush.HasValue)
                    subscription.EnablePush = request.EnablePush.Value;
                if (request.EnableEmail.HasValue)
                    subscription.EnableEmail = request.EnableEmail.Value;
                if (request.EnableSms.HasValue)
                    subscription.EnableSms = request.EnableSms.Value;
                if (request.QuietHoursStart.HasValue)
                    subscription.QuietHoursStart = request.QuietHoursStart.Value;
                if (request.QuietHoursEnd.HasValue)
                    subscription.QuietHoursEnd = request.QuietHoursEnd.Value;

                subscription.UpdatedBy = request.UserId;
                subscription.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _subscriptionRepo.UpdateAsync(subscription, ct);

                _logger.LogInformation(
                    "User {UserId} updated alert preferences for area {AreaId}",
                    request.UserId, request.AreaId);

                return new UpdateAlertPreferencesResponse
                {
                    Success = true,
                    Message = "Preferences updated successfully"
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