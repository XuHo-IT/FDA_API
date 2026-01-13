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
        private readonly IValidator<GetAreaRequest> _validator;
        private readonly IAreaMapper _areaMapper;

        public GetAreaHandler(
            IAreaRepository areaRepository, 
            IValidator<GetAreaRequest> validator,
            IAreaMapper areaMapper)
        {
            _areaRepository = areaRepository;
            _validator = validator;
            _areaMapper = areaMapper;
        }

        public async Task<GetAreaResponse> Handle(GetAreaRequest request, CancellationToken ct)
        {
            var validationResult = await _validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return new GetAreaResponse
                {
                    Success = false,
                    Message = string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    StatusCode = AreaStatusCode.BadRequest
                };
            }

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
