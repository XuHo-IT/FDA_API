using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG106_StationComponentUpdate
{
    public class UpdateStationComponentHandler : IRequestHandler<UpdateStationComponentRequest, StationComponentResponse>
    {
        private readonly IStationComponentRepository _repository;

        public UpdateStationComponentHandler(IStationComponentRepository repository)
        {
            _repository = repository;
        }

        public async Task<StationComponentResponse> Handle(UpdateStationComponentRequest request, CancellationToken ct)
        {
            try
            {
                var component = await _repository.GetByIdAsync(request.Id, ct);
                if (component == null)
                    return new StationComponentResponse(false, $"Component with ID {request.Id} not found");

                if (!string.IsNullOrEmpty(request.Status) && !StationComponentStatuses.All.Contains(request.Status))
                    return new StationComponentResponse(false, $"Invalid status. Must be one of: {string.Join(", ", StationComponentStatuses.All)}");

                if (request.Name != null) component.Name = request.Name;
                if (request.Model != null) component.Model = request.Model;
                if (request.SerialNumber != null) component.SerialNumber = request.SerialNumber;
                if (request.FirmwareVersion != null) component.FirmwareVersion = request.FirmwareVersion;
                if (request.Status != null) component.Status = request.Status;
                if (request.Notes != null) component.Notes = request.Notes;

                component.UpdatedBy = Guid.Empty;
                component.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(component, ct);
                return new StationComponentResponse(true, "Component updated successfully", request.Id, component);
            }
            catch (Exception ex)
            {
                return new StationComponentResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}
