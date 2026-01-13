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

            // Authorization check: Only the owner can update the area
            if (area.UserId != request.UserId)
            {
                return new UpdateAreaResponse
                {
                    Success = false,
                    Message = "Unauthorized to update this area",
                    StatusCode = AreaStatusCode.Forbidden
                };
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
                StatusCode = AreaStatusCode.Success
            };
        }
    }
}
