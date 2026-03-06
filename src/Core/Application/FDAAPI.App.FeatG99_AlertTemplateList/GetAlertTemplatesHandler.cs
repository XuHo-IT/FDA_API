using FDAAPI.App.Common.Features;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG99_AlertTemplateList
{
    public class GetAlertTemplatesHandler : IRequestHandler<GetAlertTemplatesRequest, GetAlertTemplatesResponse>
    {
        private readonly IAlertTemplateRepository _repository;

        public GetAlertTemplatesHandler(IAlertTemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetAlertTemplatesResponse> Handle(
            GetAlertTemplatesRequest request,
            CancellationToken ct)
        {
            try
            {
                var templates = (await _repository.GetAllAsync(
                    request.IsActive,
                    request.Channel,
                    request.Severity,
                    ct)).ToList();

                return new GetAlertTemplatesResponse(
                    true,
                    "Templates retrieved successfully",
                    templates);
            }
            catch (Exception ex)
            {
                return new GetAlertTemplatesResponse(
                    false,
                    $"Error: {ex.Message}",
                    new List<AlertTemplate>());
            }
        }
    }
}
