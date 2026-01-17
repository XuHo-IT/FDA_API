using MediatR;

namespace FDAAPI.App.FeatG43_DispatchNotifications
{
    public record DispatchNotificationsRequest() : IRequest<DispatchNotificationsResponse>;
}