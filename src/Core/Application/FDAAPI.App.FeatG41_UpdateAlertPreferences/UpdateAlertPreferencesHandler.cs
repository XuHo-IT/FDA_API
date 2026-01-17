using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG41_UpdateAlertPreferences
{
    public class UpdateAlertPreferencesHandler : IRequestHandler<UpdateAlertPreferencesRequest, UpdateAlertPreferencesResponse>
    {
        private readonly IUserAlertSubscriptionRepository _subscriptionRepo;

        public UpdateAlertPreferencesHandler(IUserAlertSubscriptionRepository subscriptionRepo)
        {
            _subscriptionRepo = subscriptionRepo;
        }

        public async Task<UpdateAlertPreferencesResponse> Handle(UpdateAlertPreferencesRequest request, CancellationToken ct)
        {
            try
            {
                // Get existing subscription
                var subscription = await _subscriptionRepo.GetByIdAsync(request.SubscriptionId, ct);
                if (subscription == null)
                {
                    return new UpdateAlertPreferencesResponse
                    {
                        Success = false,
                        Message = "Subscription not found"
                    };
                }

                // Authorization: Check if subscription belongs to user
                if (subscription.UserId != request.UserId)
                {
                    return new UpdateAlertPreferencesResponse
                    {
                        Success = false,
                        Message = "Unauthorized: Subscription does not belong to this user"
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

                subscription.UserId = request.UserId;
                subscription.UpdatedAt = DateTime.UtcNow;

                // Save changes
                await _subscriptionRepo.UpdateAsync(subscription, ct);

                return new UpdateAlertPreferencesResponse
                {
                    Success = true,
                    Message = "Preferences updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new UpdateAlertPreferencesResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}