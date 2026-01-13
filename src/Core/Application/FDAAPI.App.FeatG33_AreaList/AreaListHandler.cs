using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Areas;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG33_AreaList
{
    public class AreaListHandler : IRequestHandler<AreaListRequest, AreaListResponse>
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IValidator<AreaListRequest> _validator;
        private readonly IAreaMapper _areaMapper;

        public AreaListHandler(
            IAreaRepository areaRepository, 
            IValidator<AreaListRequest> validator,
            IAreaMapper areaMapper)
        {
            _areaRepository = areaRepository;
            _validator = validator;
            _areaMapper = areaMapper;
        }

        public async Task<AreaListResponse> Handle(AreaListRequest request, CancellationToken ct)
        {
            var validationResult = await _validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return new AreaListResponse
                {
                    Success = false,
                    Message = string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    StatusCode = AreaStatusCode.BadRequest
                };
            }

            var (areas, totalCount) = await _areaRepository.GetByUserIdAsync(request.UserId, request.SearchTerm, request.PageNumber, request.PageSize, ct);

            return new AreaListResponse
            {
                Success = true,
                Message = "Areas retrieved successfully",
                StatusCode = AreaStatusCode.Success,
                Areas = _areaMapper.MapToDtoList(areas),
                TotalCount = totalCount
            };
        }
    }
}
