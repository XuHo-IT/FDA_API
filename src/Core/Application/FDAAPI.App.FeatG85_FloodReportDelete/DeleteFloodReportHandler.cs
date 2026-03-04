using FDAAPI.App.Common.Services;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG85_FloodReportDelete
{
    public sealed class DeleteFloodReportHandler : IRequestHandler<DeleteFloodReportRequest, DeleteFloodReportResponse>
    {
        private readonly IFloodReportRepository _reportRepository;
        private readonly IFloodReportMediaRepository _mediaRepository;
        private readonly IImageStorageService _imageStorage;
        private readonly IVideoStorageService _videoStorage;
        private readonly ILogger<DeleteFloodReportHandler> _logger;

        public DeleteFloodReportHandler(
            IFloodReportRepository reportRepository,
            IFloodReportMediaRepository mediaRepository,
            IImageStorageService imageStorage,
            IVideoStorageService videoStorage,
            ILogger<DeleteFloodReportHandler> logger)
        {
            _reportRepository = reportRepository;
            _mediaRepository = mediaRepository;
            _imageStorage = imageStorage;
            _videoStorage = videoStorage;
            _logger = logger;
        }

        public async Task<DeleteFloodReportResponse> Handle(DeleteFloodReportRequest request, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Deleting flood report {ReportId} by user {UserId}", 
                    request.Id, request.UserId);

                var report = await _reportRepository.GetByIdAsync(request.Id, ct);

                if (report == null)
                {
                    _logger.LogWarning("Flood report {ReportId} not found for deletion", request.Id);
                    return new DeleteFloodReportResponse
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
                    _logger.LogWarning("Unauthorized delete attempt for report {ReportId} by user {UserId}", 
                        request.Id, request.UserId);
                    return new DeleteFloodReportResponse
                    {
                        Success = false,
                        Message = "Flood report not found"
                    };
                }

                // Get associated media records BEFORE deleting from DB
                var mediaRecords = await _mediaRepository.GetByReportIdAsync(report.Id, ct);
                
                // Delete media files from cloud storage FIRST
                if (mediaRecords != null && mediaRecords.Any())
                {
                    foreach (var media in mediaRecords)
                    {
                        try
                        {
                            if (media.MediaType == "photo")
                            {
                                var fileId = ExtractFileIdFromUrl(media.MediaUrl);
                                if (!string.IsNullOrEmpty(fileId))
                                {
                                    await _imageStorage.DeleteImageAsync(fileId);
                                    _logger.LogInformation("Deleted photo {FileId} from ImageKit", fileId);
                                }
                            }
                            else if (media.MediaType == "video")
                            {
                                var fileId = ExtractFileIdFromUrl(media.MediaUrl);
                                if (!string.IsNullOrEmpty(fileId))
                                {
                                    await _videoStorage.DeleteVideoAsync(fileId, ct);
                                    _logger.LogInformation("Deleted video {FileId} from Cloudinary", fileId);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log warning but don't stop deletion - DB cleanup should still happen
                            _logger.LogWarning(ex, 
                                "Failed to delete cloud file {FileId} for media {MediaId}", 
                                ExtractFileIdFromUrl(media.MediaUrl), media.Id);
                        }

                        // Delete media record from DB
                        try
                        {
                            await _mediaRepository.DeleteAsync(media.Id, ct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to delete media record {MediaId}", media.Id);
                            throw;
                        }
                    }
                }

                // Delete the report itself
                var result = await _reportRepository.DeleteAsync(request.Id, ct);

                if (!result)
                {
                    _logger.LogError("Failed to delete flood report {ReportId} from database", request.Id);
                    return new DeleteFloodReportResponse
                    {
                        Success = false,
                        Message = "Failed to delete flood report"
                    };
                }

                _logger.LogInformation("Flood report {ReportId} deleted successfully", request.Id);
                return new DeleteFloodReportResponse
                {
                    Success = true,
                    Message = "Flood report deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting flood report {ReportId}", request.Id);
                return new DeleteFloodReportResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting the flood report"
                };
            }
        }

        /// <summary>
        /// Extracts file ID from cloud storage URL for deletion purposes.
        /// Handles both ImageKit and Cloudinary URL formats.
        /// </summary>
        private string? ExtractFileIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            try
            {
                // Try to parse as URI
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    // Remove leading/trailing slashes and return path as file ID
                    var path = uri.AbsolutePath.Trim('/');
                    return string.IsNullOrEmpty(path) ? null : path;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract file ID from URL: {Url}", url);
            }

            return null;
        }
    }
}
