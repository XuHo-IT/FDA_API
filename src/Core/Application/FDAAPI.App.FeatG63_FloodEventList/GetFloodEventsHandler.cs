using FDAAPI.App.Common.Models.FloodEvents;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG63_FloodEventList
{
    public class GetFloodEventsHandler : IRequestHandler<GetFloodEventsRequest, GetFloodEventsResponse>
    {
        private readonly IFloodEventRepository _repository;
        private readonly IFloodEventMapper _mapper;

        public GetFloodEventsHandler(
            IFloodEventRepository repository,
            IFloodEventMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<GetFloodEventsResponse> Handle(GetFloodEventsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var (events, totalCount) = await _repository.GetFloodEventsAsync(
                    request.SearchTerm,
                    request.AdministrativeAreaId,
                    request.StartDate,
                    request.EndDate,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

                return new GetFloodEventsResponse
                {
                    Success = true,
                    Message = "Flood events retrieved successfully",
                    StatusCode = FloodEventStatusCode.Success,
                    FloodEvents = _mapper.MapToDtoList(events),
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                return new GetFloodEventsResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = FloodEventStatusCode.UnknownError
                };
            }
        }
    }
}

