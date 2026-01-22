using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FDAAPI.App.FeatG68_DeleteSubscription
{
    public class DeleteSubscriptionHandler : IRequestHandler<DeleteSubscriptionRequest, DeleteSubscriptionResponse>
    {
        private readonly IUserAlertSubscriptionRepository _subscriptionRepo;
        private readonly ILogger<DeleteSubscriptionHandler> _logger;

        public DeleteSubscriptionHandler(
            IUserAlertSubscriptionRepository subscriptionRepo,
            ILogger<DeleteSubscriptionHandler> logger)
        {
            _subscriptionRepo = subscriptionRepo;
            _logger = logger;
        }

        public async Task<DeleteSubscriptionResponse> Handle(
            DeleteSubscriptionRequest request,
            CancellationToken ct)
        {
            try
            {
                // ===== STEP 1: Get subscription to check ownership =====
                var subscription = await _subscriptionRepo.GetByIdAsync(request.SubscriptionId, ct);

                if (subscription == null)
                {
                    return new DeleteSubscriptionResponse
                    {
                        Success = false,
                        Message = "Subscription not found"
                    };
                }

                // ===== STEP 2: Check ownership =====
                // Only owner can delete their subscription
                if (subscription.UserId != request.UserId)
                {
                    return new DeleteSubscriptionResponse
                    {
                        Success = false,
                        Message = "You don't have permission to delete this subscription"
                    };
                }

                // ===== STEP 3: Delete subscription =====
                var deleted = await _subscriptionRepo.DeleteAsync(request.SubscriptionId, ct);

                if (!deleted)
                {
                    return new DeleteSubscriptionResponse
                    {
                        Success = false,
                        Message = "Failed to delete subscription"
                    };
                }

                _logger.LogInformation(
                    "Subscription {SubscriptionId} deleted by user {UserId}",
                    request.SubscriptionId, request.UserId);

                return new DeleteSubscriptionResponse
                {
                    Success = true,
                    Message = "Subscription deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error deleting subscription {SubscriptionId}",
                    request.SubscriptionId);

                return new DeleteSubscriptionResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}