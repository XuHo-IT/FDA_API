using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG50_GetJobStatus
{
    public sealed record GetJobStatusRequest(
        Guid JobRunId
    ) : IFeatureRequest<GetJobStatusResponse>;
}

