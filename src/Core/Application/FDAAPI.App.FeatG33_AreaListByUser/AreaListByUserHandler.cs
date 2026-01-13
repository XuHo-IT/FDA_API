using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Areas;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG33_AreaListByUser
{
    public class AreaListByUserHandler : IRequestHandler<AreaListByUserRequest, AreaListByUserResponse>
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IAreaMapper _areaMapper;

        public AreaListByUserHandler(
            IAreaRepository areaRepository, 
            IAreaMapper areaMapper)
        {
            _areaRepository = areaRepository;
            _areaMapper = areaMapper;
        }

        public async Task<AreaListByUserResponse> Handle(AreaListByUserRequest request, CancellationToken ct)
        {
            var (areas, totalCount) = await _areaRepository.GetByUserIdAsync(request.UserId, request.SearchTerm, request.PageNumber, request.PageSize, ct);

            return new AreaListByUserResponse
            {
                Success = true,
                Message = "User areas retrieved successfully",
                StatusCode = AreaStatusCode.Success,
                Areas = _areaMapper.MapToDtoList(areas),
                TotalCount = totalCount
            };
        }
    }
}
