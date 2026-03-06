using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG105_StationComponentCreate
{
    public class CreateStationComponentHandler : IRequestHandler<CreateStationComponentRequest, StationComponentResponse>
    {
        private readonly IStationComponentRepository _repository;
        private readonly IStationRepository _stationRepository;

        public CreateStationComponentHandler(
            IStationComponentRepository repository,
            IStationRepository stationRepository)
        {
            _repository = repository;
            _stationRepository = stationRepository;
        }

        public async Task<StationComponentResponse> Handle(CreateStationComponentRequest request, CancellationToken ct)
        {
            try
            {
                var station = await _stationRepository.GetByIdAsync(request.StationId, ct);
                if (station == null)
                    return new StationComponentResponse(false, $"Station with ID {request.StationId} not found");

                if (!StationComponentTypes.All.Contains(request.ComponentType))
                    return new StationComponentResponse(false, $"Invalid component type. Must be one of: {string.Join(", ", StationComponentTypes.All)}");

                var exists = await _repository.ExistsByTypeAsync(request.StationId, request.ComponentType, ct);
                if (exists)
                    return new StationComponentResponse(false, $"Component type '{request.ComponentType}' already exists in this station");

                var component = new StationComponent
                {
                    Id = Guid.NewGuid(),
                    StationId = request.StationId,
                    ComponentType = request.ComponentType,
                    Name = request.Name,
                    Model = request.Model,
                    SerialNumber = request.SerialNumber,
                    FirmwareVersion = request.FirmwareVersion,
                    Status = StationComponentStatuses.Active,
                    Notes = request.Notes,
                    CreatedBy = Guid.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = Guid.Empty,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await _repository.CreateAsync(component, ct);
                return new StationComponentResponse(true, "Component created successfully", created.Id, created);
            }
            catch (Exception ex)
            {
                return new StationComponentResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}
