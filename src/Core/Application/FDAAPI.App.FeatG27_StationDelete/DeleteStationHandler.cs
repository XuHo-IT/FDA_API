using FDAAPI.App.Common.Features;
using FDAAPI.App.Common.Models.Stations;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using FluentValidation;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG27_StationDelete
{
    public class DeleteStationHandler : IRequestHandler<DeleteStationRequest, DeleteStationResponse>
    {
        private readonly IStationRepository _stationRepository;
        private readonly IValidator<DeleteStationRequest> _validator;
        private readonly IStationMapper _stationMapper;

        public DeleteStationHandler(
            IStationRepository stationRepository,
            IValidator<DeleteStationRequest> validator,
            IStationMapper stationMapper)
        {
            _stationRepository = stationRepository;
            _validator = validator;
            _stationMapper = stationMapper;
        }

        public async Task<DeleteStationResponse> Handle(DeleteStationRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new DeleteStationResponse
                {
                    Success = false,
                    Message = string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    StatusCode = StationStatusCode.InvalidData
                };
            }

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
