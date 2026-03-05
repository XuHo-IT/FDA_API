namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat74_RequestSafeRoute.DTOs
{
    public class RequestSafeRouteRequestDto
    {
        public decimal StartLatitude { get; set; }
        public decimal StartLongitude { get; set; }
        public decimal EndLatitude { get; set; }
        public decimal EndLongitude { get; set; }
        public string RouteProfile { get; set; } = "car";
        public int MaxAlternatives { get; set; } = 3;
        public bool AvoidFloodedAreas { get; set; } = true;
    }

}
