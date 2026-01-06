using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat21_StationCreate.DTOs
{
    public class CreateStationReponseDto
    {
        public Guid? Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

