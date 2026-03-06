using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Features;
using System;

namespace FDAAPI.App.FeatG79_FloodReportCreate
{
    public sealed class CreateFloodReportResponse : IFeatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Status of the created report: published | hidden | escalated
        /// </summary>
        public string Status { get; set; } = "published";

        /// <summary>
        /// Confidence level: low | medium | high
        /// </summary>
        public string ConfidenceLevel { get; set; } = "medium";

        /// <summary>
        /// Calculated Trust Score (0-100).
        /// </summary>
        public int TrustScore { get; set; }

        /// <summary>
        /// Created report id.
        /// </summary>
        public Guid? Id { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}


