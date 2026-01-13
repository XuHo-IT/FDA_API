using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Areas;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using FluentValidation;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG35_AreaGet
{
    public class GetAreaHandler : IRequestHandler<GetAreaRequest, GetAreaResponse>
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IAreaMapper _areaMapper;

        public GetAreaHandler(
            IAreaRepository areaRepository, 
            IAreaMapper areaMapper)
        {
            _areaRepository = areaRepository;
            _areaMapper = areaMapper;
        }

        public async Task<GetAreaResponse> Handle(GetAreaRequest request, CancellationToken ct)
        {
            var area = await _areaRepository.GetByIdAsync(request.Id, ct);

            if (area == null)
            {
                return new GetAreaResponse
                {
                    Success = false,
                    Message = "Area not found",
                    StatusCode = AreaStatusCode.NotFound
                };
            }

            return new GetAreaResponse
            {
                Success = true,
                Message = "Area retrieved successfully",
                StatusCode = AreaStatusCode.Success,
                Area = _areaMapper.MapToDto(area)
            };
        }
    }
}
