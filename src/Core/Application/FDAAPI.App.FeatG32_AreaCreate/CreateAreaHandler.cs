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

        public CreateAreaHandler(
            IAreaRepository areaRepository, 
            IAreaMapper areaMapper)
        {
            _areaRepository = areaRepository;
            _areaMapper = areaMapper;
        }

        public async Task<CreateAreaResponse> Handle(CreateAreaRequest request, CancellationToken ct)
        {
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

            await _areaRepository.CreateAsync(area, ct);

            return new CreateAreaResponse
            {
                Success = true,
                Message = "Area created successfully",
                StatusCode = AreaStatusCode.Created,
                Data = _areaMapper.MapToDto(area)
            };
        }
    }
}
