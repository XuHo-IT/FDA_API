using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG104_AlertTemplatePreview
{
    public class PreviewAlertTemplateHandler : IRequestHandler<PreviewAlertTemplateRequest, PreviewAlertTemplateResponse>
    {
        private readonly IAlertTemplateRepository _repository;

        public PreviewAlertTemplateHandler(IAlertTemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task<PreviewAlertTemplateResponse> Handle(
            PreviewAlertTemplateRequest request,
            CancellationToken ct)
        {
            try
            {
                string? titleTemplate = request.TitleTemplate;
                string? bodyTemplate = request.BodyTemplate;

                if (request.TemplateId.HasValue)
                {
                    var template = await _repository.GetByIdAsync(request.TemplateId.Value, ct);
                    if (template == null)
                        return new PreviewAlertTemplateResponse(false, "Template not found");

                    titleTemplate = template.TitleTemplate;
                    bodyTemplate = template.BodyTemplate;
                }

                if (string.IsNullOrWhiteSpace(titleTemplate) || string.IsNullOrWhiteSpace(bodyTemplate))
                    return new PreviewAlertTemplateResponse(false, "TitleTemplate and BodyTemplate are required");

                var title = RenderTemplate(titleTemplate, request);
                var body = RenderTemplate(bodyTemplate, request);

                return new PreviewAlertTemplateResponse(true, "Success", title, body);
            }
            catch (Exception ex)
            {
                return new PreviewAlertTemplateResponse(false, $"Error: {ex.Message}");
            }
        }

        private string RenderTemplate(string template, PreviewAlertTemplateRequest data)
        {
            var result = template;

            result = result.Replace("{{station_name}}", data.StationName ?? "Unknown");
            result = result.Replace("{{water_level}}", $"{data.WaterLevel:F2}m");
            result = result.Replace("{{water_level_raw}}", data.WaterLevel.ToString("F2"));
            result = result.Replace("{{severity}}", data.Severity?.ToLower() ?? "unknown");
            result = result.Replace("{{time}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
            result = result.Replace("{{threshold}}", data.Threshold.ToString("F2"));
            result = result.Replace("{{address}}", data.Address ?? "Unknown");
            result = result.Replace("{{message}}", data.Message ?? "");

            return result;
        }
    }
}
