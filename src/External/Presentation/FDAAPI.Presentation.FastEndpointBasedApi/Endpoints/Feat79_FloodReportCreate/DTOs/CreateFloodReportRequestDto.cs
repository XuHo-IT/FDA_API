using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat79_FloodReportCreate.DTOs
{
    public sealed class CreateFloodReportRequestDto
    {
        [Required]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [RegularExpression("low|medium|high", ErrorMessage = "Severity must be low, medium, or high")]
        public string Severity { get; set; } = "medium";

        /// <summary>
        /// Optional photos for the flood report (multiple files allowed).
        /// Form field name should be 'photos'.
        /// </summary>
        public List<IFormFile>? Photos { get; set; }

        /// <summary>
        /// Optional videos for the flood report (multiple files allowed).
        /// Form field name should be 'videos'.
        /// </summary>
        public List<IFormFile>? Videos { get; set; }
    }
}


