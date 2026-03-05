using FDAAPI.App.Common.Features;

namespace FDAAPI.App.FeatG85_FloodReportDelete
{
    public sealed class DeleteFloodReportResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
