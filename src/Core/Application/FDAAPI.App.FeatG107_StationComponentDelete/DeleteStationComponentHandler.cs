using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG107_StationComponentDelete
{
    public class DeleteStationComponentHandler : IRequestHandler<DeleteStationComponentRequest, StationComponentResponse>
    {
        private readonly IStationComponentRepository _repository;

        public DeleteStationComponentHandler(IStationComponentRepository repository)
        {
            _repository = repository;
        }

        public async Task<StationComponentResponse> Handle(DeleteStationComponentRequest request, CancellationToken ct)
        {
            try
            {
                var component = await _repository.GetByIdAsync(request.Id, ct);
                if (component == null)
                    return new StationComponentResponse(false, $"Component with ID {request.Id} not found");

                var result = await _repository.DeleteAsync(request.Id, ct);
                return result
                    ? new StationComponentResponse(true, "Component deleted successfully")
                    : new StationComponentResponse(false, "Failed to delete component");
            }
            catch (Exception ex)
            {
                return new StationComponentResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}
