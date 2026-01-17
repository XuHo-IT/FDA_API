namespace FDAAPI.Infra.Services.Notifications
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(
            string phoneNumber,
            string message,
            CancellationToken ct = default);
    }
}