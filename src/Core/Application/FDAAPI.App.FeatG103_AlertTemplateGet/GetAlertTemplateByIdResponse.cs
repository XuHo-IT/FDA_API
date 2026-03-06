using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.FeatG103_AlertTemplateGet
{
    public record GetAlertTemplateByIdResponse(
        bool Success,
        string Message,
        AlertTemplate? Template = null
    );
}
