using FDAAPI.App.Common.Models.FloodEvents;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG66_FloodEventDelete
{
    public class DeleteFloodEventHandler : IRequestHandler<DeleteFloodEventRequest, DeleteFloodEventResponse>
    {
        private readonly IFloodEventRepository _repository;

        public DeleteFloodEventHandler(
            IFloodEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<DeleteFloodEventResponse> Handle(DeleteFloodEventRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if event exists
                var floodEvent = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (floodEvent == null)
                {
                    return new DeleteFloodEventResponse
                    {
                        Success = false,
                        Message = "Flood event not found",
                        StatusCode = FloodEventStatusCode.NotFound
                    };
                }

                var success = await _repository.DeleteAsync(request.Id, cancellationToken);
                if (!success)
                {
                    return new DeleteFloodEventResponse
                    {
                        Success = false,
                        Message = "Flood event could not be deleted",
                        StatusCode = FloodEventStatusCode.UnknownError
                    };
                }

                return new DeleteFloodEventResponse
                {
                    Success = true,
                    Message = "Flood event deleted successfully",
                    StatusCode = FloodEventStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                return new DeleteFloodEventResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = FloodEventStatusCode.UnknownError
                };
            }
        }
    }
}

