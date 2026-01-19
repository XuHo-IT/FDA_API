using FDAAPI.App.Common.Models.AdministrativeAreas;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG59_AdministrativeAreaGet
{
    public class GetAdministrativeAreaHandler : IRequestHandler<GetAdministrativeAreaRequest, GetAdministrativeAreaResponse>
    {
        private readonly IAdministrativeAreaRepository _repository;
        private readonly IAdministrativeAreaMapper _mapper;

        public GetAdministrativeAreaHandler(
            IAdministrativeAreaRepository repository,
            IAdministrativeAreaMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<GetAdministrativeAreaResponse> Handle(GetAdministrativeAreaRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var area = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (area == null)
                {
                    return new GetAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "Administrative area not found",
                        StatusCode = AdministrativeAreaStatusCode.NotFound
                    };
                }

                return new GetAdministrativeAreaResponse
                {
                    Success = true,
                    Message = "Administrative area retrieved successfully",
                    StatusCode = AdministrativeAreaStatusCode.Success,
                    AdministrativeArea = _mapper.MapToDto(area)
                };
            }
            catch (Exception ex)
            {
                return new GetAdministrativeAreaResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = AdministrativeAreaStatusCode.UnknownError
                };
            }
        }
    }
}

