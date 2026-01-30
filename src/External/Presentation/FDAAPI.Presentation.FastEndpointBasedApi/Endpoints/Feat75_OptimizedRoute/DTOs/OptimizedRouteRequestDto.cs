using FDAAPI.App.Common.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat75_OptimizedRoute.DTOs
{
    public class OptimizedRouteRequestDto
    {
        public decimal StartLatitude { get; set; }
        public decimal StartLongitude { get; set; }
        public decimal EndLatitude { get; set; }
        public decimal EndLongitude { get; set; }
        public string RouteProfile { get; set; } = "car";
        public int MaxAlternatives { get; set; } = 3;
        public bool AvoidFloodedAreas { get; set; } = true;
        public List<WaypointDto>? Waypoints { get; set; }
        public DateTime? DepartureTime { get; set; }
    }
}
