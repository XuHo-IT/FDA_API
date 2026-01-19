using FDAAPI.App.Common.Models.FloodEvents;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG62_FloodEventCreate
{
    public class CreateFloodEventHandler : IRequestHandler<CreateFloodEventRequest, CreateFloodEventResponse>
    {
        private readonly IFloodEventRepository _repository;
        private readonly IAdministrativeAreaRepository _areaRepository;
        private readonly IFloodEventMapper _mapper;

        public CreateFloodEventHandler(
            IFloodEventRepository repository,
            IAdministrativeAreaRepository areaRepository,
            IFloodEventMapper mapper)
        {
            _repository = repository;
            _areaRepository = areaRepository;
            _mapper = mapper;
        }

        public async Task<CreateFloodEventResponse> Handle(CreateFloodEventRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate administrative area exists
                var area = await _areaRepository.GetByIdAsync(request.AdministrativeAreaId, cancellationToken);
                if (area == null)
                {
                    return new CreateFloodEventResponse
                    {
                        Success = false,
                        Message = "Administrative area not found",
                        StatusCode = FloodEventStatusCode.NotFound
                    };
                }

                // Validate time range
                if (request.StartTime >= request.EndTime)
                {
                    return new CreateFloodEventResponse
                    {
                        Success = false,
                        Message = "Start time must be before end time",
                        StatusCode = FloodEventStatusCode.InvalidData
                    };
                }

                // Calculate duration if not provided
                var durationHours = request.DurationHours;
                if (!durationHours.HasValue)
                {
                    durationHours = (int)(request.EndTime - request.StartTime).TotalHours;
                }

                var floodEvent = new FloodEvent
                {
                    Id = Guid.NewGuid(),
                    AdministrativeAreaId = request.AdministrativeAreaId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    PeakLevel = request.PeakLevel,
                    DurationHours = durationHours,
                    CreatedAt = DateTime.UtcNow
                };

                var id = await _repository.CreateAsync(floodEvent, cancellationToken);
                floodEvent.Id = id;

                // Reload with administrative area for mapping
                var createdEvent = await _repository.GetByIdAsync(id, cancellationToken);
                if (createdEvent == null)
                {
                    return new CreateFloodEventResponse
                    {
                        Success = false,
                        Message = "Flood event was created but could not be retrieved",
                        StatusCode = FloodEventStatusCode.UnknownError
                    };
                }

                return new CreateFloodEventResponse
                {
                    Success = true,
                    Message = "Flood event created successfully",
                    StatusCode = FloodEventStatusCode.Created,
                    Data = _mapper.MapToDto(createdEvent)
                };
            }
            catch (Exception ex)
            {
                return new CreateFloodEventResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = FloodEventStatusCode.UnknownError
                };
            }
        }
    }
}

