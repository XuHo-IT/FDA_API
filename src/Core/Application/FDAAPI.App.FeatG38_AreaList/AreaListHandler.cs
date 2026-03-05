using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Areas;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG38_AreaList
{
    public class AreaListHandler : IRequestHandler<AreaListRequest, AreaListResponse>
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IAreaMapper _areaMapper;

        public AreaListHandler(
            IAreaRepository areaRepository, 
            IAreaMapper areaMapper)
        {
            _areaRepository = areaRepository;
            _areaMapper = areaMapper;
        }

        public async Task<AreaListResponse> Handle(AreaListRequest request, CancellationToken ct)
        {
            var (areas, totalCount) = await _areaRepository.GetAdminAreasAsync(
                request.SearchTerm, 
                request.PageNumber, 
                request.PageSize, 
                ct);

            return new AreaListResponse
            {
                Success = true,
                Message = "Admin areas retrieved successfully",
                StatusCode = AreaStatusCode.Success,
                Areas = _areaMapper.MapToDtoList(areas),
                TotalCount = totalCount
            };
        }
    }
}

