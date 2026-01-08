using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Stations;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG24_StationUpdate
{
    public class UpdateStationHandler : IRequestHandler<UpdateStationRequest, UpdateStationResponse>
    {
        private readonly IStationRepository _stationRepository;

        public UpdateStationHandler(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        public async Task<UpdateStationResponse> Handle(UpdateStationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var station = await _stationRepository.GetByIdAsync(request.Id, cancellationToken);
                if (station == null)
                {
                    return new UpdateStationResponse
                    {
                        Success = false,
                        Message = "Station not found",
                        StatusCode = StationStatusCode.InvalidData // Or a NotFound status if added to enum
                    };
                }

                station.Code = request.Code;
                station.Name = request.Name;
                station.LocationDesc = request.LocationDesc;
                station.Latitude = request.Latitude;
                station.Longitude = request.Longitude;
                station.RoadName = request.RoadName;
                station.Direction = request.Direction;
                station.Status = request.Status;
                station.InstalledAt = request.InstalledAt;
                station.LastSeenAt = request.LastSeenAt;
                station.UpdatedAt = DateTime.UtcNow;
                station.UpdatedBy = request.AdminId;

                await _stationRepository.UpdateAsync(station, cancellationToken);

                return new UpdateStationResponse
                {
                    Success = true,
                    Message = "Station updated successfully",
                    StatusCode = StationStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                return new UpdateStationResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = StationStatusCode.UnknownError
                };
            }
        }
    }
}

