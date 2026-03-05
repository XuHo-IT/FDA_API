using FDAAPI.App.Common.DTOs;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FDAAPI.App.FeatG69_AdminGetAllSubscriptions
{
    public class AdminGetAllSubscriptionsHandler : IRequestHandler<AdminGetAllSubscriptionsRequest, AdminGetAllSubscriptionsResponse>
    {
        private readonly IUserAlertSubscriptionRepository _subscriptionRepo;
        private readonly ILogger<AdminGetAllSubscriptionsHandler> _logger;

        public AdminGetAllSubscriptionsHandler(
            IUserAlertSubscriptionRepository subscriptionRepo,
            ILogger<AdminGetAllSubscriptionsHandler> logger)
        {
            _subscriptionRepo = subscriptionRepo;
            _logger = logger;
        }

        public async Task<AdminGetAllSubscriptionsResponse> Handle(
            AdminGetAllSubscriptionsRequest request,
            CancellationToken ct)
        {
            try
            {
                // Validate pagination parameters
                var page = Math.Max(1, request.Page);
                var pageSize = Math.Clamp(request.PageSize, 1, 100);

                // Get paginated subscriptions
                var (items, totalCount) = await _subscriptionRepo.GetAllWithPaginationAsync(
                    page,
                    pageSize,
                    request.UserId,
                    request.StationId,
                    ct);

                // Map to DTOs
                var subscriptionDtos = items.Select(s => new AdminSubscriptionDto
                {
                    SubscriptionId = s.Id,
                    UserId = s.UserId,
                    UserEmail = s.User?.Email ?? "Unknown",
                    UserPhone = s.User?.PhoneNumber,
                    StationId = s.StationId,
                    StationName = s.Station?.Name,
                    AreaId = s.AreaId,
                    AreaName = s.Area?.Name,
                    MinSeverity = s.MinSeverity,
                    EnablePush = s.EnablePush,
                    EnableEmail = s.EnableEmail,
                    EnableSms = s.EnableSms,
                    CreatedAt = s.CreatedAt
                }).ToList();

                // Calculate pagination info
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return new AdminGetAllSubscriptionsResponse
                {
                    Success = true,
                    Message = "Retrieved successfully",
                    Subscriptions = subscriptionDtos,
                    Pagination = new PaginationDto
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalPages = totalPages,
                        TotalCount = totalCount
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all subscriptions");
                return new AdminGetAllSubscriptionsResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}