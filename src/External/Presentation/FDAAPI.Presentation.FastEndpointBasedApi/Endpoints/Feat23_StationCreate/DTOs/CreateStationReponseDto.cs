using FDAAPI.App.Common.DTOs;
using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat21_StationCreate.DTOs
{
    public class CreateStationReponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }

        public StationDto? Data { get; set; }
    }
}

