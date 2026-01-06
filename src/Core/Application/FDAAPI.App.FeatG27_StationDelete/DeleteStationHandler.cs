using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Stations;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG26_StationDelete
{
    public class DeleteStationHandler : IRequestHandler<DeleteStationRequest, DeleteStationResponse>
    {
        private readonly IStationRepository _stationRepository;

        public DeleteStationHandler(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        public async Task<DeleteStationResponse> Handle(DeleteStationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var success = await _stationRepository.DeleteAsync(request.Id, cancellationToken);
                if (!success)
                {
                    return new DeleteStationResponse
                    {
                        Success = false,
                        Message = "Station not found or could not be deleted",
                        StatusCode = StationStatusCode.InvalidData
                    };
                }

                return new DeleteStationResponse
                {
                    Success = true,
                    Message = "Station deleted successfully",
                    StatusCode = StationStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                return new DeleteStationResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = StationStatusCode.UnknownError
                };
            }
        }
    }
}

