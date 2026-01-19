using FDAAPI.App.Common.Models.FloodEvents;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG65_FloodEventUpdate
{
    public class UpdateFloodEventHandler : IRequestHandler<UpdateFloodEventRequest, UpdateFloodEventResponse>
    {
        private readonly IFloodEventRepository _repository;
        private readonly IAdministrativeAreaRepository _areaRepository;

        public UpdateFloodEventHandler(
            IFloodEventRepository repository,
            IAdministrativeAreaRepository areaRepository)
        {
            _repository = repository;
            _areaRepository = areaRepository;
        }

        public async Task<UpdateFloodEventResponse> Handle(UpdateFloodEventRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var floodEvent = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (floodEvent == null)
                {
                    return new UpdateFloodEventResponse
                    {
                        Success = false,
                        Message = "Flood event not found",
                        StatusCode = FloodEventStatusCode.NotFound
                    };
                }

                // Validate administrative area exists
                var area = await _areaRepository.GetByIdAsync(request.AdministrativeAreaId, cancellationToken);
                if (area == null)
                {
                    return new UpdateFloodEventResponse
                    {
                        Success = false,
                        Message = "Administrative area not found",
                        StatusCode = FloodEventStatusCode.NotFound
                    };
                }

                // Validate time range
                if (request.StartTime >= request.EndTime)
                {
                    return new UpdateFloodEventResponse
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

                // Update properties
                floodEvent.AdministrativeAreaId = request.AdministrativeAreaId;
                floodEvent.StartTime = request.StartTime;
                floodEvent.EndTime = request.EndTime;
                floodEvent.PeakLevel = request.PeakLevel;
                floodEvent.DurationHours = durationHours;

                await _repository.UpdateAsync(floodEvent, cancellationToken);

                return new UpdateFloodEventResponse
                {
                    Success = true,
                    Message = "Flood event updated successfully",
                    StatusCode = FloodEventStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                return new UpdateFloodEventResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = FloodEventStatusCode.UnknownError
                };
            }
        }
    }
}

