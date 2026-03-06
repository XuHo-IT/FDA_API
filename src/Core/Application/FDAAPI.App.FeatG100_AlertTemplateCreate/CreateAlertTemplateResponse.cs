using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.FeatG100_AlertTemplateCreate
{
    public record CreateAlertTemplateResponse(
        bool Success,
        string Message,
        Guid? Id = null,
        AlertTemplate? Template = null
    );
}
