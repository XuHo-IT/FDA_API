using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG102_AlertTemplateDelete
{
    public class DeleteAlertTemplateHandler : IRequestHandler<DeleteAlertTemplateRequest, DeleteAlertTemplateResponse>
    {
        private readonly IAlertTemplateRepository _repository;

        public DeleteAlertTemplateHandler(IAlertTemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task<DeleteAlertTemplateResponse> Handle(
            DeleteAlertTemplateRequest request,
            CancellationToken ct)
        {
            try
            {
                var template = await _repository.GetByIdAsync(request.Id, ct);
                if (template == null)
                    return new DeleteAlertTemplateResponse(false, "Template not found");

                await _repository.DeleteAsync(request.Id, ct);

                return new DeleteAlertTemplateResponse(true, "Template deleted successfully");
            }
            catch (Exception ex)
            {
                return new DeleteAlertTemplateResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}
