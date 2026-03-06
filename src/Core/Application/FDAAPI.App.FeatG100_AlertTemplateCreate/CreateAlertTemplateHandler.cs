using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;

namespace FDAAPI.App.FeatG100_AlertTemplateCreate
{
    public class CreateAlertTemplateHandler : IRequestHandler<CreateAlertTemplateRequest, CreateAlertTemplateResponse>
    {
        private readonly IAlertTemplateRepository _repository;

        public CreateAlertTemplateHandler(IAlertTemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task<CreateAlertTemplateResponse> Handle(
            CreateAlertTemplateRequest request,
            CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    return new CreateAlertTemplateResponse(false, "Name is required");

                if (string.IsNullOrWhiteSpace(request.Channel))
                    return new CreateAlertTemplateResponse(false, "Channel is required");

                if (string.IsNullOrWhiteSpace(request.TitleTemplate))
                    return new CreateAlertTemplateResponse(false, "TitleTemplate is required");

                if (string.IsNullOrWhiteSpace(request.BodyTemplate))
                    return new CreateAlertTemplateResponse(false, "BodyTemplate is required");

                var validChannels = new[] { "Push", "Email", "SMS", "InApp" };
                if (!validChannels.Contains(request.Channel, StringComparer.OrdinalIgnoreCase))
                    return new CreateAlertTemplateResponse(false, $"Invalid channel. Must be one of: {string.Join(", ", validChannels)}");

                if (!string.IsNullOrWhiteSpace(request.Severity))
                {
                    var validSeverities = new[] { "info", "caution", "warning", "critical" };
                    if (!validSeverities.Contains(request.Severity.ToLower()))
                        return new CreateAlertTemplateResponse(false, $"Invalid severity. Must be one of: {string.Join(", ", validSeverities)}");
                }

                var template = new AlertTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Channel = request.Channel,
                    Severity = request.Severity,
                    TitleTemplate = request.TitleTemplate,
                    BodyTemplate = request.BodyTemplate,
                    IsActive = request.IsActive,
                    SortOrder = request.SortOrder,
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = request.CreatedBy,
                    UpdatedAt = DateTime.UtcNow
                };

                var id = await _repository.CreateAsync(template, ct);

                return new CreateAlertTemplateResponse(true, "Template created successfully", id, template);
            }
            catch (Exception ex)
            {
                return new CreateAlertTemplateResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}
