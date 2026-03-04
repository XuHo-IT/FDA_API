using FDAAPI.App.Common.Features;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;

namespace FDAAPI.App.FeatG79_FloodReportCreate
{
    /// <summary>
    /// Core data for creating a flood report.
    /// Files (photos/videos) are provided separately from the Endpoint via IFormFile collections.
    /// </summary>
    public sealed record CreateFloodReportRequest(
        Guid? UserId,
        decimal Latitude,
        decimal Longitude,
        string? Address,
        string? Description,
        string Severity
    ) : IFeatureRequest<CreateFloodReportResponse>;

    /// <summary>
    /// Wrapper used at the endpoint level to pass files into the handler.
    /// </summary>
    public sealed record CreateFloodReportWithFilesCommand(
        CreateFloodReportRequest CoreRequest,
        IFormFileCollection? Photos,
        IFormFileCollection? Videos
    ) : IRequest<CreateFloodReportResponse>;
}


