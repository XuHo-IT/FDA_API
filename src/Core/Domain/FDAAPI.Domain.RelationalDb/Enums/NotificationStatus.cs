namespace FDAAPI.Domain.RelationalDb.Enums
{
    public enum NotificationStatus
    {
        Pending = 0,
        Sending = 1,
        Sent = 2,
        Delivered = 3,
        Failed = 4,
        Retrying = 5,
        Read = 6
    }
}