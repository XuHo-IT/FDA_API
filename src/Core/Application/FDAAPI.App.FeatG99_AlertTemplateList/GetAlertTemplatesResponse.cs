using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.FeatG99_AlertTemplateList
{
    public record GetAlertTemplatesResponse(
        bool Success,
        string Message,
        List<AlertTemplate> Templates
    );
}
