using FastEndpoints;
using FDAAPI.App.FeatG82_FloodReportUpdate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat82_FloodReportUpdate.DTOs;
using MediatR;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat82_FloodReportUpdate
{
    public class UpdateFloodReportEndpoint : Endpoint<UpdateFloodReportRequestDto, UpdateFloodReportResponseDto>
    {
        private readonly IMediator _mediator;

        public UpdateFloodReportEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/api/v1/flood-reports/{id}");
            AllowFileUploads();
            Policies("User");
            Summary(s =>
            {
                s.Summary = "Update a flood report";
                s.Description = "Update description, address, severity, add new media files, or delete existing media by ID.";
            });
            Description(b => b
                .Produces(200)
                .Produces(400)
                .Produces(401)
                .Produces(404));
            Tags("FloodReports");
        }

        public override async Task HandleAsync(UpdateFloodReportRequestDto req, CancellationToken ct)
        {
            // Extract report ID from route
            var reportId = Route<Guid>("id");

            // Get user info from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

            if (userIdClaim == null)
            {
                await SendAsync(new UpdateFloodReportResponseDto
                {
                    Success = false,
                    Message = "Unauthorized"
                }, 401, ct);
                return;
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "USER";

            // Extract files from multipart/form-data (same as create logic)
            List<IFormFile>? photos = null;
            List<IFormFile>? videos = null;

            if (HttpContext.Request.HasFormContentType)
            {
                var allFormFiles = HttpContext.Request.Form.Files;
                var photoFiles = new List<IFormFile>();
                var videoFiles = new List<IFormFile>();

                foreach (var file in allFormFiles)
                {
                    if (string.Equals(file.Name, "photos", StringComparison.OrdinalIgnoreCase))
                    {
                        photoFiles.Add(file);
                    }
                    else if (string.Equals(file.Name, "videos", StringComparison.OrdinalIgnoreCase))
                    {
                        videoFiles.Add(file);
                    }
                    else if (string.Equals(file.Name, "mediaFilesToAdd", StringComparison.OrdinalIgnoreCase))
                    {
                        // Accept legacy field for compatibility
                        photoFiles.Add(file);
                    }
                }

                if (photoFiles.Count > 0)
                {
                    photos = photoFiles;
                }
                if (videoFiles.Count > 0)
                {
                    videos = videoFiles;
                }
            }
            // Robustly handle empty or invalid mediaToDelete from multipart form-data
            var validMediaToDelete = new List<Guid>();
            if (HttpContext.Request.Form.ContainsKey("mediaToDelete"))
            {
                var raw = HttpContext.Request.Form["mediaToDelete"].ToString();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    validMediaToDelete = new List<Guid>();
                }
                else
                {
                    // Try to parse as JSON array, fallback to comma-separated string
                    try
                    {
                        var parsed = System.Text.Json.JsonSerializer.Deserialize<List<string>>(raw);
                        if (parsed != null)
                        {
                            foreach (var s in parsed)
                            {
                                if (Guid.TryParse(s, out var guid) && guid != Guid.Empty)
                                    validMediaToDelete.Add(guid);
                            }
                        }
                    }
                    catch
                    {
                        // Fallback: try comma-separated
                        var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var part in parts)
                        {
                            if (Guid.TryParse(part.Trim(), out var guid) && guid != Guid.Empty)
                                validMediaToDelete.Add(guid);
                        }
                    }
                }
            }
            else if (req.MediaToDelete != null)
            {
                validMediaToDelete = req.MediaToDelete.Where(g => g != Guid.Empty).ToList();
            }

            var command = new UpdateFloodReportWithFilesCommand(
                reportId,
                userId,
                userRole,
                req.Address,
                req.Description,
                req.Severity,
                photos,
                videos,
                validMediaToDelete
            );

            // Send to handler
            var result = await _mediator.Send(command, ct);

            // Map response
            var response = new UpdateFloodReportResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Id = result.Id,
                Latitude = result.Latitude,
                Longitude = result.Longitude,
                Address = result.Address,
                Description = result.Description,
                Severity = result.Severity,
                TrustScore = result.TrustScore,
                Status = result.Status,
                ConfidenceLevel = result.ConfidenceLevel,
                Priority = result.Priority,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                Media = result.Media?.ConvertAll(m => new FloodReportMediaDto
                {
                    Id = m.Id,
                    MediaType = m.MediaType,
                    MediaUrl = m.MediaUrl,
                    ThumbnailUrl = m.ThumbnailUrl,
                    CreatedAt = m.CreatedAt
                }) ?? new()
            };

            // Return appropriate status code
            var statusCode = result.Success ? 200 : 400;
            await SendAsync(response, statusCode, ct);
        }
    }
}
