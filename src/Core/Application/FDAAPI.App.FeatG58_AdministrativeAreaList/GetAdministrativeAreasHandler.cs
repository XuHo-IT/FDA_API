using FDAAPI.App.Common.Models.AdministrativeAreas;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG58_AdministrativeAreaList
{
    public class GetAdministrativeAreasHandler : IRequestHandler<GetAdministrativeAreasRequest, GetAdministrativeAreasResponse>
    {
        private readonly IAdministrativeAreaRepository _repository;
        private readonly IAdministrativeAreaMapper _mapper;

        public GetAdministrativeAreasHandler(
            IAdministrativeAreaRepository repository,
            IAdministrativeAreaMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<GetAdministrativeAreasResponse> Handle(GetAdministrativeAreasRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var (areas, totalCount) = await _repository.GetAdministrativeAreasAsync(
                    request.SearchTerm,
                    request.Level,
                    request.ParentId,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

                return new GetAdministrativeAreasResponse
                {
                    Success = true,
                    Message = "Administrative areas retrieved successfully",
                    StatusCode = AdministrativeAreaStatusCode.Success,
                    AdministrativeAreas = _mapper.MapToDtoList(areas),
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                return new GetAdministrativeAreasResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = AdministrativeAreaStatusCode.UnknownError
                };
            }
        }
    }
}

