namespace FDAAPI.App.FeatG104_AlertTemplatePreview
{
    public record PreviewAlertTemplateResponse(
        bool Success,
        string Message,
        string? Title = null,
        string? Body = null
    );
}
