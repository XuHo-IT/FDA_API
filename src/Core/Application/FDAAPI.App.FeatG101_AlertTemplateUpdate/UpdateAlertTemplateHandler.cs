using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG101_AlertTemplateUpdate
{
    public class UpdateAlertTemplateHandler : IRequestHandler<UpdateAlertTemplateRequest, UpdateAlertTemplateResponse>
    {
        private readonly IAlertTemplateRepository _repository;

        public UpdateAlertTemplateHandler(IAlertTemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task<UpdateAlertTemplateResponse> Handle(
            UpdateAlertTemplateRequest request,
            CancellationToken ct)
        {
            try
            {
                var template = await _repository.GetByIdAsync(request.Id, ct);
                if (template == null)
                    return new UpdateAlertTemplateResponse(false, "Template not found");

                var validChannels = new[] { "Push", "Email", "SMS", "InApp" };
                if (!validChannels.Contains(request.Channel, StringComparer.OrdinalIgnoreCase))
                    return new UpdateAlertTemplateResponse(false, "Invalid channel");

                if (!string.IsNullOrWhiteSpace(request.Severity))
                {
                    var validSeverities = new[] { "info", "caution", "warning", "critical" };
                    if (!validSeverities.Contains(request.Severity.ToLower()))
                        return new UpdateAlertTemplateResponse(false, "Invalid severity");
                }

                template.Name = request.Name;
                template.Channel = request.Channel;
                template.Severity = request.Severity;
                template.TitleTemplate = request.TitleTemplate;
                template.BodyTemplate = request.BodyTemplate;
                template.IsActive = request.IsActive;
                template.SortOrder = request.SortOrder;
                template.UpdatedBy = request.UpdatedBy;
                template.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(template, ct);

                return new UpdateAlertTemplateResponse(true, "Template updated successfully", template);
            }
            catch (Exception ex)
            {
                return new UpdateAlertTemplateResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}
