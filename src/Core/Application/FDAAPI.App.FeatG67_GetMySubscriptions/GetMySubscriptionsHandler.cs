using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FDAAPI.App.FeatG67_GetMySubscriptions
{
    public class GetMySubscriptionsHandler : IRequestHandler<GetMySubscriptionsRequest, GetMySubscriptionsResponse>
    {
        private readonly IUserAlertSubscriptionRepository _subscriptionRepo;
        private readonly ILogger<GetMySubscriptionsHandler> _logger;

        public GetMySubscriptionsHandler(
            IUserAlertSubscriptionRepository subscriptionRepo,
            ILogger<GetMySubscriptionsHandler> logger)
        {
            _subscriptionRepo = subscriptionRepo;
            _logger = logger;
        }

        public async Task<GetMySubscriptionsResponse> Handle(
            GetMySubscriptionsRequest request,
            CancellationToken ct)
        {
            try
            {
                var subscriptions = await _subscriptionRepo.GetByUserIdAsync(request.UserId, ct);

                var subscriptionDtos = subscriptions.Select(s => new UserSubscriptionDto
                {
                    SubscriptionId = s.Id,
                    StationId = s.StationId,
                    StationName = s.Station?.Name ?? "Unknown Station",
                    AreaId = s.AreaId,
                    AreaName = s.Area?.Name,
                    MinSeverity = s.MinSeverity,
                    EnablePush = s.EnablePush,
                    EnableEmail = s.EnableEmail,
                    EnableSms = s.EnableSms,
                    CreatedAt = s.CreatedAt
                }).ToList();

                return new GetMySubscriptionsResponse
                {
                    Success = true,
                    Message = "Retrieved successfully",
                    Subscriptions = subscriptionDtos,
                    TotalCount = subscriptionDtos.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions for user {UserId}", request.UserId);
                return new GetMySubscriptionsResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}