using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FDAAPI.App.FeatG39_SubscribeToAlerts
{
    public class SubscribeToAlertsHandler : IRequestHandler<SubscribeToAlertsRequest, SubscribeToAlertsResponse>
    {
        private readonly IUserAlertSubscriptionRepository _subscriptionRepo;
        private readonly IAreaRepository _areaRepo;
        private readonly IStationRepository _stationRepo;
        private readonly ILogger<SubscribeToAlertsHandler> _logger;

        public SubscribeToAlertsHandler(
            IUserAlertSubscriptionRepository subscriptionRepo,
            IAreaRepository areaRepo,
            IStationRepository stationRepo,
            ILogger<SubscribeToAlertsHandler> logger)
        {
            _subscriptionRepo = subscriptionRepo;
            _areaRepo = areaRepo;
            _stationRepo = stationRepo;
            _logger = logger;
        }

        public async Task<SubscribeToAlertsResponse> Handle(
            SubscribeToAlertsRequest request,
            CancellationToken ct)
        {
            try
            {
                // ===== BUSINESS RULE: Phải có AreaId HOẶC StationId (ít nhất 1 trong 2) =====
                if (!request.AreaId.HasValue && !request.StationId.HasValue)
                {
                    return new SubscribeToAlertsResponse
                    {
                        Success = false,
                        Message = "Either AreaId or StationId must be provided"
                    };
                }

                // ===== VALIDATION: Nếu có AreaId, kiểm tra Area tồn tại và thuộc về user =====
                if (request.AreaId.HasValue)
                {
                    var area = await _areaRepo.GetByIdAsync(request.AreaId.Value, ct);
                    if (area == null)
                    {
                        return new SubscribeToAlertsResponse
                        {
                            Success = false,
                            Message = "Area not found"
                        };
                    }

                    if (area.UserId != request.UserId)
                    {
                        return new SubscribeToAlertsResponse
                        {
                            Success = false,
                            Message = "You can only subscribe to your own areas"
                        };
                    }
                }

                // ===== VALIDATION: Nếu có StationId, kiểm tra Station tồn tại =====
                if (request.StationId.HasValue)
                {
                    var station = await _stationRepo.GetByIdAsync(request.StationId.Value, ct);
                    if (station == null)
                    {
                        return new SubscribeToAlertsResponse
                        {
                            Success = false,
                            Message = "Station not found"
                        };
                    }
                }

                // ===== CHECK: Subscription đã tồn tại chưa? =====
                var existingSubscriptions = await _subscriptionRepo.GetByUserIdAsync(request.UserId, ct);

                var duplicate = existingSubscriptions.FirstOrDefault(s =>
                    (request.AreaId.HasValue && s.AreaId == request.AreaId) ||
                    (request.StationId.HasValue && s.StationId == request.StationId));

                if (duplicate != null)
                {
                    return new SubscribeToAlertsResponse
                    {
                        Success = false,
                        Message = request.AreaId.HasValue
                            ? "You are already subscribed to alerts for this area"
                            : "You are already subscribed to alerts for this station"
                    };
                }

                var subscription = new UserAlertSubscription
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    AreaId = request.AreaId,
                    StationId = request.StationId,
                    MinSeverity = request.MinSeverity,
                    EnablePush = request.EnablePush,
                    EnableEmail = request.EnableEmail,
                    EnableSms = request.EnableSms,
                    CreatedBy = request.UserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = request.UserId,
                    UpdatedAt = DateTime.UtcNow
                };

                await _subscriptionRepo.CreateAsync(subscription, ct);

                return new SubscribeToAlertsResponse
                {
                    Success = true,
                    Message = request.AreaId.HasValue
                        ? "Successfully subscribed to area alerts"
                        : "Successfully subscribed to station alerts",
                    SubscriptionId = subscription.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to alerts");
                return new SubscribeToAlertsResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}