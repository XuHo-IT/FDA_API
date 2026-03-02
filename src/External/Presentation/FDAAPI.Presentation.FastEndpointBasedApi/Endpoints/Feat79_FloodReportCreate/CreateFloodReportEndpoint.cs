using FastEndpoints;
using FDAAPI.App.FeatG79_FloodReportCreate;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat79_FloodReportCreate.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat79_FloodReportCreate
{
    public sealed class CreateFloodReportEndpoint
        : Endpoint<CreateFloodReportRequestDto, CreateFloodReportResponseDto>
    {
        private readonly IMediator _mediator;

        public CreateFloodReportEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/api/v1/flood-reports");
            // Only authenticated users can create reports (see HandleAsync)
            AllowFileUploads();
            Summary(s =>
            {
                s.Summary = "Create community flood report (authenticated only)";
                s.Description = "Uploads a community flood report with optional photos/videos. Only authenticated users are allowed to create reports.";
            });
            Tags("FloodReports", "Community");
        }

        public override async Task HandleAsync(CreateFloodReportRequestDto req, CancellationToken ct)
        {
            // Resolve user id if authenticated
                // Only authenticated users are allowed to create flood reports
                Guid? userId = null;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsed))
                {
                    userId = parsed;
                }
                if (userId == null)
                {
                    await SendAsync(new CreateFloodReportResponseDto
                    {
                        Success = false,
                        Message = "You must be logged in to create a flood report."
                    }, 401, ct);
                    return;
                }

                // Map to core request
                var coreRequest = new CreateFloodReportRequest(
                    userId,
                    req.Latitude,
                    req.Longitude,
                    req.Address,
                    req.Description,
                    req.Severity
                );

                // Extract files from multipart/form-data
            IFormFileCollection? photos = null;
            IFormFileCollection? videos = null;

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
                }

                // Use a custom wrapper that implements IFormFileCollection
                if (photoFiles.Count > 0)
                {
                    photos = new FormFileCollectionAdapter(photoFiles);
                }

                if (videoFiles.Count > 0)
                {
                    videos = new FormFileCollectionAdapter(videoFiles);
                }
            }

            var command = new CreateFloodReportWithFilesCommand(coreRequest, photos, videos);

            var result = await _mediator.Send(command, ct);

            var statusCode = result.Success ? 201 : 400;

            await SendAsync(new CreateFloodReportResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                Id = result.Id,
                Status = result.Status,
                ConfidenceLevel = result.ConfidenceLevel,
                TrustScore = result.TrustScore,
                CreatedAt = result.CreatedAt
            }, statusCode, ct);
        }

        /// <summary>
        /// Adapter class to wrap a List<IFormFile> as IFormFileCollection
        /// </summary>
        private class FormFileCollectionAdapter : IFormFileCollection
        {
            private readonly List<IFormFile> _files;

            public FormFileCollectionAdapter(List<IFormFile> files)
            {
                _files = files ?? new List<IFormFile>();
            }

            public IFormFile? this[string name] => _files.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));

            public IFormFile? this[int index] => index < _files.Count ? _files[index] : null;

            public int Count => _files.Count;

            public IEnumerator<IFormFile> GetEnumerator() => _files.GetEnumerator();

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _files.GetEnumerator();

            public IReadOnlyList<IFormFile> GetFiles(string name) 
                => _files.Where(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();

            public IFormFile? GetFile(string name) 
                => _files.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}


