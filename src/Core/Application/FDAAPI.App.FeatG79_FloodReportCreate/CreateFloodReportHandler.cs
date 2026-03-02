using FDAAPI.App.Common.Services;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG79_FloodReportCreate
{
    /// <summary>
    /// Handles the create flood report use-case including media upload and basic TrustScore.
    /// </summary>
    public sealed class CreateFloodReportHandler :
        IRequestHandler<CreateFloodReportWithFilesCommand, CreateFloodReportResponse>
    {
        private readonly IFloodReportRepository _reportRepository;
        private readonly IFloodReportMediaRepository _mediaRepository;
        private readonly IImageStorageService _imageStorage;
        private readonly IVideoStorageService _videoStorage;
        private readonly ILogger<CreateFloodReportHandler> _logger;

        public CreateFloodReportHandler(
            IFloodReportRepository reportRepository,
            IFloodReportMediaRepository mediaRepository,
            IImageStorageService imageStorage,
            IVideoStorageService videoStorage,
            ILogger<CreateFloodReportHandler> logger)
        {
            _reportRepository = reportRepository;
            _mediaRepository = mediaRepository;
            _imageStorage = imageStorage;
            _videoStorage = videoStorage;
            _logger = logger;
        }

        public async Task<CreateFloodReportResponse> Handle(
            CreateFloodReportWithFilesCommand command,
            CancellationToken ct)
        {
            var request = command.CoreRequest;
            var uploadedMedia = new List<(string Type, string Url, string? ThumbnailUrl, string? FileId)>();

                // Validate severity
                var allowedSeverities = new[] { "low", "medium", "high" };
                if (string.IsNullOrWhiteSpace(request.Severity) || !allowedSeverities.Contains(request.Severity.Trim().ToLower()))
                {
                    return new CreateFloodReportResponse
                    {
                        Success = false,
                        Message = "Invalid severity value. Allowed values are: low, medium, high."
                    };
                }
            try
            {
                // 1. Validate & upload photos
                if (command.Photos != null && command.Photos.Any())
                {
                    foreach (var photo in command.Photos)
                    {
                        if (photo.Length == 0)
                            continue;

                        using var stream = photo.OpenReadStream();
                        var url = await _imageStorage.UploadImageAsync(
                            stream,
                            photo.FileName,
                            folder: "flood-reports/photos");

                        uploadedMedia.Add(("photo", url, null, ExtractFileIdFromUrl(url)));
                    }
                }

                // 2. Validate & upload videos
                if (command.Videos != null && command.Videos.Any())
                {
                    foreach (var video in command.Videos)
                    {
                        if (video.Length == 0)
                            continue;

                        using var stream = video.OpenReadStream();
                        (string videoUrl, string? thumbnailUrl) = await _videoStorage.UploadVideoAsync(
                            stream,
                            video.FileName,
                            folder: "flood-reports/videos",
                            ct);

                        uploadedMedia.Add(("video", videoUrl, thumbnailUrl, ExtractFileIdFromUrl(videoUrl)));
                    }
                }

                // 3. Basic TrustScore calculation (can be refined later)
                var trustScore = CalculateBasicTrustScore(
                    request.UserId,
                    hasMedia: uploadedMedia.Any());

                var (status, confidence) = DetermineStatus(trustScore);

                // 4. Create FloodReport entity
                var report = new FloodReport
                {
                    Id = Guid.NewGuid(),
                    ReporterUserId = request.UserId,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Address = request.Address,
                    Description = request.Description,
                    Severity = request.Severity,
                    TrustScore = trustScore,
                    Status = status,
                    ConfidenceLevel = confidence,
                    Priority = "normal",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var reportId = await _reportRepository.CreateAsync(report, ct);

                // 5. Create media records
                foreach (var media in uploadedMedia)
                {
                    var entity = new FloodReportMedia
                    {
                        Id = Guid.NewGuid(),
                        FloodReportId = reportId,
                        MediaType = media.Type,
                        MediaUrl = media.Url,
                        ThumbnailUrl = media.ThumbnailUrl,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _mediaRepository.CreateAsync(entity, ct);
                }

                return new CreateFloodReportResponse
                {
                    Success = true,
                    Message = "Flood report created successfully",
                    Id = reportId,
                    Status = status,
                    ConfidenceLevel = confidence,
                    TrustScore = trustScore,
                    CreatedAt = report.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create flood report, rolling back uploaded media.");

                await RollbackUploadedMediaAsync(uploadedMedia, ct);

                return new CreateFloodReportResponse
                {
                    Success = false,
                    Message = "Failed to create flood report. Please try again."
                };
            }
        }

        private int CalculateBasicTrustScore(Guid? userId, bool hasMedia)
        {
            // Very simple initial heuristic:
            // base 40, +15 if has media, +10 if authenticated
            var score = 40;

            if (hasMedia)
                score += 15;

            if (userId.HasValue)
                score += 10;

            return Math.Max(0, Math.Min(100, score));
        }

        private (string Status, string Confidence) DetermineStatus(int trustScore)
        {
            if (trustScore >= 60)
                return ("published", "high");
            if (trustScore >= 30)
                return ("published", "low");
            return ("hidden", "low");
        }

        private async Task RollbackUploadedMediaAsync(
            List<(string Type, string Url, string? ThumbnailUrl, string? FileId)> uploadedMedia,
            CancellationToken ct)
        {
            foreach (var media in uploadedMedia)
            {
                try
                {
                    if (media.Type == "photo" && !string.IsNullOrEmpty(media.FileId))
                    {
                        await _imageStorage.DeleteImageAsync(media.FileId);
                    }
                    else if (media.Type == "video" && !string.IsNullOrEmpty(media.FileId))
                    {
                        await _videoStorage.DeleteVideoAsync(media.FileId!, ct);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to rollback media {FileId}", media.FileId);
                }
            }
        }

        private string? ExtractFileIdFromUrl(string url)
        {
            // For ImageKit we can treat the path as fileId, for Cloudinary we expect public_id to be encoded in URL.
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return uri.AbsolutePath.TrimStart('/');
            }

            return null;
        }
    }
}


