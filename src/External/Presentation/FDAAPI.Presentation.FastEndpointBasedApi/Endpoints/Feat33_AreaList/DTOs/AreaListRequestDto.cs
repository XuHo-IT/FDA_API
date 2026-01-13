namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat33_AreaList.DTOs
{
    public class AreaListRequestDto
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

