using FDAAPI.App.Common.Models.Stations;
using FDAAPI.App.FeatG23_StationCreate;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG23_StationCreate
{
    public class CreateStationHandler : IRequestHandler<CreateStationRequest, CreateStationResponse>
    {
        private readonly IStationRepository _stationRepository;
        public CreateStationHandler(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }
        public async Task<CreateStationResponse> Handle(CreateStationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var station = new Station
                {
                    Id = Guid.NewGuid(),
                    Code = request.Code,
                    Name = request.Name,
                    LocationDesc = request.LocationDesc,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    RoadName = request.RoadName,
                    Direction = request.Direction,
                    Status = request.Status,
                    InstalledAt = request.InstalledAt,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.AdminId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = request.AdminId
                };

                await _stationRepository.CreateAsync(station, cancellationToken);

                return new CreateStationResponse
                {
                    Success = true,
                    Message = "Station created successfully",
                    StatusCode = StationStatusCode.Success,
                    StationId = station.Id
                };
            }
            catch (Exception ex)
            {
                return new CreateStationResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = StationStatusCode.UnknownError
                };
            }
        }
    }
}
