using FDAAPI.App.Common.Models.Areas;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FluentValidation;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG32_AreaCreate
{
    public class CreateAreaHandler : IRequestHandler<CreateAreaRequest, CreateAreaResponse>
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IAreaMapper _areaMapper;
        private readonly IUserAlertSubscriptionRepository _subscriptionRepo;
        private readonly IStationRepository _stationRepository;

        public CreateAreaHandler(
            IAreaRepository areaRepository,
            IAreaMapper areaMapper,
            IUserAlertSubscriptionRepository subscriptionRepository,
            IStationRepository stationRepository)
        {
            _areaRepository = areaRepository;
            _areaMapper = areaMapper;
            _subscriptionRepo = subscriptionRepository;
            _stationRepository = stationRepository;
        }

        public async Task<CreateAreaResponse> Handle(CreateAreaRequest request, CancellationToken ct)
        {
            // ===== BUSINESS RULE 1: Area Limit Check =====
            var areaCount = await _areaRepository.CountByUserIdAsync(request.UserId, ct);

            // TODO: Get user's subscription tier (Free vs Premium)
            // For now, hardcode free tier limit
            const int FREE_TIER_LIMIT = 5;

            if (areaCount >= FREE_TIER_LIMIT)
            {
                return new CreateAreaResponse
                {
                    Success = false,
                    Message = $"You have reached the maximum limit of {FREE_TIER_LIMIT} areas. Upgrade to premium for unlimited areas.",
                    StatusCode = AreaStatusCode.TooManyRequests
                };
            }

            // ===== BUSINESS RULE 2: Name Uniqueness Check =====
            var existingAreaWithSameName = await _areaRepository
                .GetByUserIdAndNameAsync(request.UserId, request.Name, ct);

            if (existingAreaWithSameName != null)
            {
                return new CreateAreaResponse
                {
                    Success = false,
                    Message = $"You already have an area named '{request.Name}'. Please choose a different name.",
                    StatusCode = AreaStatusCode.Conflict
                };
            }

            // ===== BUSINESS RULE 3: Duplicate Location Prevention =====
            var userAreas = await _areaRepository
                .GetUserAreasWithinRadiusAsync(
                    request.UserId,
                    request.Latitude,
                    request.Longitude,
                    request.RadiusMeters + 50, // Buffer for proximity check
                    ct);

            foreach (var existingArea in userAreas)
            {
                var distance = CalculateHaversineDistance(
                    request.Latitude, request.Longitude,
                    existingArea.Latitude, existingArea.Longitude);

                // If within 50m and radius overlaps, reject
                if (distance <= 50)
                {
                    return new CreateAreaResponse
                    {
                        Success = false,
                        Message = $"An area '{existingArea.Name}' already exists within 50 meters of this location.",
                        StatusCode = AreaStatusCode.Conflict
                    };
                }
            }

            // ===== BUSINESS RULE 4: Station Coverage Check =====
            var stationsInArea = await _stationRepository
                .GetStationsWithinRadiusAsync(
                    request.Latitude,
                    request.Longitude,
                    request.RadiusMeters,
                    ct);

            if (stationsInArea == null || stationsInArea.Count == 0)
            {
                return new CreateAreaResponse
                {
                    Success = false,
                    Message = "Cannot create area: No active monitoring stations found within the specified radius. Please choose a location with station coverage.",
                    StatusCode = AreaStatusCode.UnprocessableEntity
                };
            }

            // ===== CREATE AREA (Existing Logic) =====
            var area = new Area
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Name = request.Name,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                RadiusMeters = request.RadiusMeters,
                AddressText = request.AddressText,
                CreatedBy = request.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = request.UserId,
                UpdatedAt = DateTime.UtcNow
            };

            var areaId = await _areaRepository.CreateAsync(area, ct);

            // AUTO-CREATE subscription with default settings
            var subscription = new UserAlertSubscription
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                AreaId = areaId,
                StationId = null,
                MinSeverity = "warning",
                EnablePush = true,
                EnableEmail = false,
                EnableSms = false,
                CreatedBy = request.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = request.UserId,
                UpdatedAt = DateTime.UtcNow
            };

            await _subscriptionRepo.CreateAsync(subscription, ct);

            return new CreateAreaResponse
            {
                Success = true,
                Message = "Area created successfully",
                StatusCode = AreaStatusCode.Created,
                Data = _areaMapper.MapToDto(area)
            };
        }

        // ===== HELPER: Haversine Distance Calculation =====
        private double CalculateHaversineDistance(
            decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371000; // Earth radius in meters

            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) *
                    Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Distance in meters
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
