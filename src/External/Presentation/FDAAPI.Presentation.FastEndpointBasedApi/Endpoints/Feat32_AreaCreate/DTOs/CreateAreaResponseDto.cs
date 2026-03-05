using FDAAPI.App.Common.DTOs;
using System;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat32_AreaCreate.DTOs
{
    public class CreateAreaResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public AreaDto? Data { get; set; }
    }
}
