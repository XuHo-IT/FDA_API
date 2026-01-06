namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat25_StationList.DTOs
{
    public class GetStationsRequestDto
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

