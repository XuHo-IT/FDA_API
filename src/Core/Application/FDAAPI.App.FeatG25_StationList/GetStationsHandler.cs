using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Stations;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG25_StationList
{
    public class GetStationsHandler : IRequestHandler<GetStationsRequest, GetStationsResponse>
    {
        private readonly IStationRepository _stationRepository;

        public GetStationsHandler(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        public async Task<GetStationsResponse> Handle(GetStationsRequest request, CancellationToken ct)
        {
            try
            {
                var (stations, totalCount) = await _stationRepository.GetStationsAsync(
                    request.SearchTerm,
                    request.Status,
                    request.PageNumber,
                    request.PageSize,
                    ct);

                var stationDtos = stations.Select(s => new StationDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name,
                    LocationDesc = s.LocationDesc,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    RoadName = s.RoadName,
                    Direction = s.Direction,
                    Status = s.Status,
                    InstalledAt = s.InstalledAt,
                    LastSeenAt = s.LastSeenAt,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                });

                return new GetStationsResponse
                {
                    Success = true,
                    Message = "Stations retrieved successfully",
                    StatusCode = StationStatusCode.Success,
                    Stations = stationDtos,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                return new GetStationsResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = StationStatusCode.UnknownError
                };
            }
        }
    }
}

