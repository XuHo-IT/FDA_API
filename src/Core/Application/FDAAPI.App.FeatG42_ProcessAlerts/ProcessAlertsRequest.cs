using MediatR;

namespace FDAAPI.App.FeatG42_ProcessAlerts
{
    public record ProcessAlertsRequest() : IRequest<ProcessAlertsResponse>;
}