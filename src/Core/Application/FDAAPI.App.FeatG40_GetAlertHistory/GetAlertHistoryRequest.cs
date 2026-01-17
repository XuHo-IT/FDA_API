using MediatR;

namespace FDAAPI.App.FeatG40_GetAlertHistory
{
    public record GetAlertHistoryRequest(
        Guid UserId,
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        string? Severity = null,          // Filter by severity: info, caution, warning, critical
        string? Status = null,            // Filter by status: sent, delivered, failed
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<GetAlertHistoryResponse>;
}