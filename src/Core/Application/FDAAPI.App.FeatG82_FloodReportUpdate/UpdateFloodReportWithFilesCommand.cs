using FDAAPI.App.Common.Features;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace FDAAPI.App.FeatG82_FloodReportUpdate
{
    public sealed record UpdateFloodReportWithFilesCommand(
        Guid Id,
        Guid? UserId,
        string UserRole,
        string? Address,
        string? Description,
        string? Severity,
        List<IFormFile>? Photos = null,
        List<IFormFile>? Videos = null,
        List<Guid>? MediaToDelete = null
    ) : IFeatureRequest<UpdateFloodReportResponse>;
}
