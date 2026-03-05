using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using FDAAPI.App.FeatG82_FloodReportUpdate;
using Microsoft.AspNetCore.Http;
using FDAAPI.App.Common.Services;

namespace FDAAPI.App.FeatG82_FloodReportUpdate
{
    public sealed class UpdateFloodReportWithFilesHandler : IRequestHandler<UpdateFloodReportWithFilesCommand, UpdateFloodReportResponse>
    {
        private readonly IFloodReportRepository _reportRepository;
        private readonly IFloodReportMediaRepository _mediaRepository;
        private readonly IImageStorageService _imageStorage;
        private readonly IVideoStorageService _videoStorage;
        private readonly ILogger<UpdateFloodReportWithFilesHandler> _logger;

        public UpdateFloodReportWithFilesHandler(
            IFloodReportRepository reportRepository,
            IFloodReportMediaRepository mediaRepository,
            IImageStorageService imageStorage,
            IVideoStorageService videoStorage,
            ILogger<UpdateFloodReportWithFilesHandler> logger)
        {
            _reportRepository = reportRepository;
            _mediaRepository = mediaRepository;
            _imageStorage = imageStorage;
            _videoStorage = videoStorage;
            _logger = logger;
        }

        public async Task<UpdateFloodReportResponse> Handle(UpdateFloodReportWithFilesCommand request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Updating flood report {ReportId} by user {UserId}", request.Id, request.UserId);

                var report = await _reportRepository.GetByIdAsync(request.Id, ct);
                if (report == null)
                {
                    _logger.LogWarning("Flood report {ReportId} not found for update", request.Id);
                    return new UpdateFloodReportResponse
                    {
                        Success = false,
                        Message = "Flood report not found"
                    };
                }

                // Authorization: Check if user is the reporter or admin
                bool isAdmin = request.UserRole == "ADMIN" || request.UserRole == "SUPERADMIN";
                if (!isAdmin && report.ReporterUserId != request.UserId)
                {
                    _logger.LogWarning("Unauthorized update attempt for report {ReportId} by user {UserId}", request.Id, request.UserId);
                    return new UpdateFloodReportResponse
                    {
                        Success = false,
                        Message = "Flood report not found"
                    };
                }

                // Update editable fields
                bool hasChanges = false;
                if (!string.IsNullOrEmpty(request.Address) && report.Address != request.Address)
                {
                    var sanitized = System.Net.WebUtility.HtmlEncode(request.Address);
                    report.Address = sanitized.Length > 500 ? sanitized[..500] : sanitized;
                    hasChanges = true;
                }
                if (!string.IsNullOrEmpty(request.Description) && report.Description != request.Description)
                {
                    var sanitized = System.Net.WebUtility.HtmlEncode(request.Description);
                    report.Description = sanitized.Length > 1000 ? sanitized[..1000] : sanitized;
                    hasChanges = true;
                }
                if (!string.IsNullOrEmpty(request.Severity) && report.Severity != request.Severity)
                {
                    if (new[] { "low", "medium", "high" }.Contains(request.Severity.ToLower()))
                    {
                        report.Severity = request.Severity.ToLower();
                        hasChanges = true;
                    }
                }

                // Media operations
                bool mediaChanged = false;
                // Delete media
                if (request.MediaToDelete != null)
                {
                    foreach (var mediaId in request.MediaToDelete)
                    {
                        await _mediaRepository.DeleteAsync(mediaId, ct);
                        mediaChanged = true;
                    }
                }

                // Add new photos
                if (request.Photos != null && request.Photos.Count > 0)
                {
                    foreach (var file in request.Photos)
                    {
                        if (file.Length == 0)
                            continue;
                        using var stream = file.OpenReadStream();
                        var url = await _imageStorage.UploadImageAsync(
                            stream,
                            file.FileName,
                            folder: "flood-reports/photos");
                        var newMedia = new FDAAPI.Domain.RelationalDb.Entities.FloodReportMedia
                        {
                            Id = Guid.NewGuid(),
                            FloodReportId = report.Id,
                            MediaType = "photo",
                            MediaUrl = url,
                            ThumbnailUrl = null,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _mediaRepository.CreateAsync(newMedia, ct);
                        mediaChanged = true;
                    }
                }
                // Add new videos
                if (request.Videos != null && request.Videos.Count > 0)
                {
                    foreach (var file in request.Videos)
                    {
                        if (file.Length == 0)
                            continue;
                        using var stream = file.OpenReadStream();
                        var (videoUrl, thumbnailUrl) = await _videoStorage.UploadVideoAsync(
                            stream,
                            file.FileName,
                            folder: "flood-reports/videos",
                            ct);
                        var newMedia = new FDAAPI.Domain.RelationalDb.Entities.FloodReportMedia
                        {
                            Id = Guid.NewGuid(),
                            FloodReportId = report.Id,
                            MediaType = "video",
                            MediaUrl = videoUrl,
                            ThumbnailUrl = thumbnailUrl,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _mediaRepository.CreateAsync(newMedia, ct);
                        mediaChanged = true;
                    }
                }

                // Log if no changes
                if (!hasChanges && !mediaChanged)
                {
                    _logger.LogInformation("No changes detected for flood report {ReportId}", request.Id);
                    var media = await _mediaRepository.GetByReportIdAsync(report.Id, ct);
                    return new UpdateFloodReportResponse
                    {
                        Success = true,
                        Message = "No changes were made",
                        Id = report.Id,
                        ReporterUserId = report.ReporterUserId,
                        Latitude = report.Latitude,
                        Longitude = report.Longitude,
                        Address = report.Address,
                        Description = report.Description,
                        Severity = report.Severity,
                        TrustScore = report.TrustScore,
                        Status = report.Status,
                        ConfidenceLevel = report.ConfidenceLevel,
                        Priority = report.Priority,
                        CreatedAt = report.CreatedAt,
                        UpdatedAt = report.UpdatedAt,
                        Media = media?.Select(m => new FloodReportMediaItem
                        {
                            Id = m.Id,
                            MediaType = m.MediaType,
                            MediaUrl = m.MediaUrl,
                            ThumbnailUrl = m.ThumbnailUrl,
                            CreatedAt = m.CreatedAt
                        }).ToList() ?? new()
                    };
                }

                report.UpdatedAt = DateTime.UtcNow;
                var result = await _reportRepository.UpdateAsync(report, ct);
                if (!result)
                {
                    _logger.LogError("Failed to update flood report {ReportId} in database", request.Id);
                    return new UpdateFloodReportResponse
                    {
                        Success = false,
                        Message = "Failed to update flood report"
                    };
                }

                var updatedMedia = await _mediaRepository.GetByReportIdAsync(report.Id, ct);
                var response = new UpdateFloodReportResponse
                {
                    Success = true,
                    Message = "Flood report updated successfully",
                    Id = report.Id,
                    ReporterUserId = report.ReporterUserId,
                    Latitude = report.Latitude,
                    Longitude = report.Longitude,
                    Address = report.Address,
                    Description = report.Description,
                    Severity = report.Severity,
                    TrustScore = report.TrustScore,
                    Status = report.Status,
                    ConfidenceLevel = report.ConfidenceLevel,
                    Priority = report.Priority,
                    CreatedAt = report.CreatedAt,
                    UpdatedAt = report.UpdatedAt,
                    Media = updatedMedia?.Select(m => new FloodReportMediaItem
                    {
                        Id = m.Id,
                        MediaType = m.MediaType,
                        MediaUrl = m.MediaUrl,
                        ThumbnailUrl = m.ThumbnailUrl,
                        CreatedAt = m.CreatedAt
                    }).ToList() ?? new()
                };
                _logger.LogInformation("Flood report {ReportId} updated successfully", request.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating flood report {ReportId}", request.Id);
                return new UpdateFloodReportResponse
                {
                    Success = false,
                    Message = $"An error occurred while updating the flood report"
                };
            }
        }
    }
}
