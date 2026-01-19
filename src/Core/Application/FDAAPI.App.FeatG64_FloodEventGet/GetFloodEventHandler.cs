using FDAAPI.App.Common.Models.FloodEvents;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG64_FloodEventGet
{
    public class GetFloodEventHandler : IRequestHandler<GetFloodEventRequest, GetFloodEventResponse>
    {
        private readonly IFloodEventRepository _repository;
        private readonly IFloodEventMapper _mapper;

        public GetFloodEventHandler(
            IFloodEventRepository repository,
            IFloodEventMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<GetFloodEventResponse> Handle(GetFloodEventRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var floodEvent = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (floodEvent == null)
                {
                    return new GetFloodEventResponse
                    {
                        Success = false,
                        Message = "Flood event not found",
                        StatusCode = FloodEventStatusCode.NotFound
                    };
                }

                return new GetFloodEventResponse
                {
                    Success = true,
                    Message = "Flood event retrieved successfully",
                    StatusCode = FloodEventStatusCode.Success,
                    FloodEvent = _mapper.MapToDto(floodEvent)
                };
            }
            catch (Exception ex)
            {
                return new GetFloodEventResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = FloodEventStatusCode.UnknownError
                };
            }
        }
    }
}

