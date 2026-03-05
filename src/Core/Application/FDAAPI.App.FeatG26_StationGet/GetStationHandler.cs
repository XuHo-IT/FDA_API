using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Stations;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using FluentValidation;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG26_StationGet
{
    public class GetStationHandler : IRequestHandler<GetStationRequest, GetStationResponse>
    {
        private readonly IStationRepository _stationRepository;
        private readonly IStationMapper _stationMapper;

        public GetStationHandler(
            IStationRepository stationRepository,
            IStationMapper stationMapper)
        {
            _stationRepository = stationRepository;
            _stationMapper = stationMapper;
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
                    Station = _stationMapper.MapToDto(station)
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
