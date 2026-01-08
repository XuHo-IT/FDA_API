using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Stations;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG26_StationGet
{
    public class GetStationHandler : IRequestHandler<GetStationRequest, GetStationResponse>
    {
        private readonly IStationRepository _stationRepository;

        public GetStationHandler(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        public async Task<GetStationResponse> Handle(GetStationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var station = await _stationRepository.GetByIdAsync(request.Id, cancellationToken);
                if (station == null)
                {
                    return new GetStationResponse
                    {
                        Success = false,
                        Message = "Station not found",
                        StatusCode = StationStatusCode.InvalidData
                    };
                }

                return new GetStationResponse
                {
                    Success = true,
                    Message = "Station retrieved successfully",
                    StatusCode = StationStatusCode.Success,
                    Station = station
                };
            }
            catch (Exception ex)
            {
                return new GetStationResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = StationStatusCode.UnknownError
                };
            }
        }
    }
}

