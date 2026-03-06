using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG103_AlertTemplateGet
{
    public class GetAlertTemplateByIdHandler : IRequestHandler<GetAlertTemplateByIdRequest, GetAlertTemplateByIdResponse>
    {
        private readonly IAlertTemplateRepository _repository;

        public GetAlertTemplateByIdHandler(IAlertTemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetAlertTemplateByIdResponse> Handle(
            GetAlertTemplateByIdRequest request,
            CancellationToken ct)
        {
            try
            {
                var template = await _repository.GetByIdAsync(request.Id, ct);
                if (template == null)
                    return new GetAlertTemplateByIdResponse(false, "Template not found");

                return new GetAlertTemplateByIdResponse(true, "Template retrieved successfully", template);
            }
            catch (Exception ex)
            {
                return new GetAlertTemplateByIdResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}
