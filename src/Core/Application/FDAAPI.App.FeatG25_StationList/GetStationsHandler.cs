using FDAAPI.App.Common.DTOs;
using FDAAPI.App.Common.Models.Stations;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG25_StationList
{
    public class GetStationsHandler : IRequestHandler<GetStationsRequest, GetStationsResponse>
    {
        private readonly IStationRepository _stationRepository;
        private readonly IStationMapper _stationMapper;

        public GetStationsHandler(
            IStationRepository stationRepository,
            IStationMapper stationMapper)
        {
            _stationRepository = stationRepository;
            _stationMapper = stationMapper;
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

                // Use mapper to convert entities to DTOs
                var stationDtos = _stationMapper.MapToDtoList(stations);

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

