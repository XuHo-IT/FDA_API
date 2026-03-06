using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG108_StationComponentList
{
    public class GetStationComponentsHandler : IRequestHandler<GetStationComponentsRequest, StationComponentListResponse>
    {
        private readonly IStationComponentRepository _repository;

        public GetStationComponentsHandler(IStationComponentRepository repository)
        {
            _repository = repository;
        }

        public async Task<StationComponentListResponse> Handle(GetStationComponentsRequest request, CancellationToken ct)
        {
            try
            {
                var components = await _repository.GetByStationIdAsync(request.StationId, ct);
                return new StationComponentListResponse(true, "Success", components);
            }
            catch (Exception ex)
            {
                return new StationComponentListResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}
