using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG39_SubscribeToAlerts
{
    public class SubscribeToAlertsHandler : IRequestHandler<SubscribeToAlertsRequest, SubscribeToAlertsResponse>
    {
        private readonly IUserAlertSubscriptionRepository _subscriptionRepo;
        private readonly IUserRepository _userRepo;

        public SubscribeToAlertsHandler(
            IUserAlertSubscriptionRepository subscriptionRepo,
            IUserRepository userRepo)
        {
            _subscriptionRepo = subscriptionRepo;
            _userRepo = userRepo;
        }

        public async Task<SubscribeToAlertsResponse> Handle(SubscribeToAlertsRequest request, CancellationToken ct)
        {
            try
            {
                // Validate: Must provide either StationId or AreaId
                if (!request.StationId.HasValue && !request.AreaId.HasValue)
                {
                    return new SubscribeToAlertsResponse
                    {
                        Success = false,
                        Message = "Must provide either StationId or AreaId"
                    };
                }

                // Check if already subscribed
                if (request.StationId.HasValue)
                {
                    var exists = await _subscriptionRepo.IsUserSubscribedAsync(request.UserId, request.StationId.Value, ct);
                    if (exists)
                    {
                        return new SubscribeToAlertsResponse
                        {
                            Success = false,
                            Message = "Already subscribed to this station"
                        };
                    }
                }

                // TODO: Check subscription tier limits (Free: 5 stations, Premium: unlimited)
                var existingCount = (await _subscriptionRepo.GetByUserIdAsync(request.UserId, ct)).Count();
                // Get user tier from pricing_plans table (placeholder for now)
                const int FREE_TIER_LIMIT = 5;
                if (existingCount >= FREE_TIER_LIMIT)
                {
                    return new SubscribeToAlertsResponse
                    {
                        Success = false,
                        Message = $"Subscription limit reached ({FREE_TIER_LIMIT}). Upgrade to Premium for unlimited."
                    };
                }

                // Create subscription
                var subscription = new UserAlertSubscription
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    StationId = request.StationId,
                    AreaId = request.AreaId,
                    MinSeverity = request.MinSeverity,
                    EnablePush = request.EnablePush,
                    EnableEmail = request.EnableEmail,
                    EnableSms = request.EnableSms,
                    CreatedBy = request.UserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = request.UserId,
                    UpdatedAt = DateTime.UtcNow
                };

                var subscriptionId = await _subscriptionRepo.CreateAsync(subscription, ct);

                return new SubscribeToAlertsResponse
                {
                    Success = true,
                    Message = "Successfully subscribed to alerts",
                    SubscriptionId = subscriptionId
                };
            }
            catch (Exception ex)
            {
                return new SubscribeToAlertsResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}