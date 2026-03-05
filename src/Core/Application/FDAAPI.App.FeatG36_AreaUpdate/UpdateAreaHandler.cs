using FDAAPI.App.Common.Models.Areas;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using FluentValidation;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG36_AreaUpdate
{
    public class UpdateAreaHandler : IRequestHandler<UpdateAreaRequest, UpdateAreaResponse>
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IAreaMapper _areaMapper;

        public UpdateAreaHandler(
            IAreaRepository areaRepository, 
            IAreaMapper areaMapper)
        {
            _areaRepository = areaRepository;
            _areaMapper = areaMapper;
        }

        public async Task<UpdateAreaResponse> Handle(UpdateAreaRequest request, CancellationToken ct)
        {
            var area = await _areaRepository.GetByIdAsync(request.Id, ct);

            if (area == null)
            {
                return new UpdateAreaResponse
                {
                    Success = false,
                    Message = "Area not found",
                    StatusCode = AreaStatusCode.NotFound
                };
            }

            // 2. Check if user is Admin or SuperAdmin
            bool isAdmin = request.UserRole == "ADMIN" || request.UserRole == "SUPERADMIN";

            // 3. Authorization check with Admin override
            if (!isAdmin && area.UserId != request.UserId)
            {
                // Return 404 instead of 403 to prevent area ID enumeration
                return new UpdateAreaResponse
                {
                    Success = false,
                    Message = "Area not found",  // ← Changed from "Unauthorized..."
                    StatusCode = AreaStatusCode.NotFound  // ← Changed from Forbidden
                };
            }

            // 4. Check duplicate name (if name is being changed)
            if (area.Name != request.Name)
            {
                var duplicateArea = await _areaRepository
                    .GetByUserIdAndNameAsync(area.UserId, request.Name, ct);

                if (duplicateArea != null && duplicateArea.Id != request.Id)
                {
                    return new UpdateAreaResponse
                    {
                        Success = false,
                        Message = $"You already have an area named '{request.Name}'. Please choose a different name.",
                        StatusCode = AreaStatusCode.Conflict
                    };
                }
            }

            area.Name = request.Name;
            area.Latitude = request.Latitude;
            area.Longitude = request.Longitude;
            area.RadiusMeters = request.RadiusMeters;
            area.AddressText = request.AddressText;
            area.UpdatedAt = DateTime.UtcNow;
            area.UpdatedBy = request.UserId;

            var result = await _areaRepository.UpdateAsync(area, ct);

            if (!result)
            {
                return new UpdateAreaResponse
                {
                    Success = false,
                    Message = "Failed to update area",
                    StatusCode = AreaStatusCode.InternalServerError
                };
            }

            return new UpdateAreaResponse
            {
                Success = true,
                Message = "Area updated successfully",
                StatusCode = AreaStatusCode.Success,
                Data = _areaMapper.MapToDto(area)
            };
        }
    }
}
