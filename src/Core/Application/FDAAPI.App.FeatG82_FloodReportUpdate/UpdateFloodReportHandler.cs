using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG82_FloodReportUpdate
{
    public sealed class UpdateFloodReportHandler : IRequestHandler<UpdateFloodReportRequest, UpdateFloodReportResponse>
    {
        private readonly IFloodReportRepository _reportRepository;
        private readonly IFloodReportMediaRepository _mediaRepository;
        private readonly ILogger<UpdateFloodReportHandler> _logger;

        public UpdateFloodReportHandler(
            IFloodReportRepository reportRepository,
            IFloodReportMediaRepository mediaRepository,
            ILogger<UpdateFloodReportHandler> logger)
        {
            _reportRepository = reportRepository;
            _mediaRepository = mediaRepository;
            _logger = logger;
        }

        public async Task<UpdateFloodReportResponse> Handle(UpdateFloodReportRequest request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Updating flood report {ReportId} by user {UserId}", 
                    request.Id, request.UserId);

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
                    // Return 404 instead of 403 to prevent report ID enumeration
                    _logger.LogWarning("Unauthorized update attempt for report {ReportId} by user {UserId}", 
                        request.Id, request.UserId);
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
                    // SECURITY: Basic XSS prevention (sanitize HTML in text)
                    var sanitized = System.Net.WebUtility.HtmlEncode(request.Address);
                    report.Address = sanitized.Length > 500 ? sanitized[..500] : sanitized;
                    hasChanges = true;
                }

                if (!string.IsNullOrEmpty(request.Description) && report.Description != request.Description)
                {
                    // SECURITY: Basic XSS prevention
                    var sanitized = System.Net.WebUtility.HtmlEncode(request.Description);
                    report.Description = sanitized.Length > 1000 ? sanitized[..1000] : sanitized;
                    hasChanges = true;
                }

                if (!string.IsNullOrEmpty(request.Severity) && report.Severity != request.Severity)
                {
                    // Validate severity is in allowed values
                    if (new[] { "low", "medium", "high" }.Contains(request.Severity.ToLower()))
                    {
                        report.Severity = request.Severity.ToLower();
                        hasChanges = true;
                    }
                }

                // Media operations
                bool mediaChanged = false;

                // Add new media
                if (request.MediaToAdd != null)
                {
                    foreach (var addItem in request.MediaToAdd)
                    {
                        var newMedia = new FDAAPI.Domain.RelationalDb.Entities.FloodReportMedia
                        {
                            Id = Guid.NewGuid(),
                            FloodReportId = report.Id,
                            MediaType = addItem.MediaType,
                            MediaUrl = addItem.MediaUrl,
                            ThumbnailUrl = addItem.ThumbnailUrl,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _mediaRepository.CreateAsync(newMedia, ct);
                        mediaChanged = true;
                    }
                }

                // Delete media
                if (request.MediaToDelete != null)
                {
                    foreach (var mediaId in request.MediaToDelete)
                    {
                        await _mediaRepository.DeleteAsync(mediaId, ct);
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

                // OPTIMIZATION: Avoid N+1 query - use report object already in memory instead of fetching again
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
