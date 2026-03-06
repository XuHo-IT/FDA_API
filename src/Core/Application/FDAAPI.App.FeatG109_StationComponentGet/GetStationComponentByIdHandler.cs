using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG109_StationComponentGet
{
    public class GetStationComponentByIdHandler : IRequestHandler<GetStationComponentByIdRequest, StationComponentResponse>
    {
        private readonly IStationComponentRepository _repository;

        public GetStationComponentByIdHandler(IStationComponentRepository repository)
        {
            _repository = repository;
        }

        public async Task<StationComponentResponse> Handle(GetStationComponentByIdRequest request, CancellationToken ct)
        {
            try
            {
                var component = await _repository.GetByIdAsync(request.Id, ct);
                if (component == null)
                    return new StationComponentResponse(false, $"Component with ID {request.Id} not found");

                return new StationComponentResponse(true, "Success", request.Id, component);
            }
            catch (Exception ex)
            {
                return new StationComponentResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}
