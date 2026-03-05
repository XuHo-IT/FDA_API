using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG82_FloodReportUpdate
{
    public sealed record UpdateFloodReportRequest(
        Guid Id,
        Guid? UserId,
        string UserRole,
        string? Address,
        string? Description,
        string? Severity,
        List<MediaAddItem>? MediaToAdd = null,
        List<Guid>? MediaToDelete = null
    ) : IFeatureRequest<UpdateFloodReportResponse>;

    public sealed record MediaAddItem(
        string MediaType,
        string MediaUrl,
        string? ThumbnailUrl = null
    );

}
