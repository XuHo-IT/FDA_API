using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.FeatG101_AlertTemplateUpdate
{
    public record UpdateAlertTemplateResponse(
        bool Success,
        string Message,
        AlertTemplate? Template = null
    );
}
