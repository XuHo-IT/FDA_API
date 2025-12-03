namespace FDAAPI.Domain.RelationalDb
{
    /// <summary>
    /// Minimal WaterLevel domain model used by feature handlers.
    /// This is a lightweight model for demo / scaffolding purposes.
    /// </summary>
    public class WaterLevel
    {
        public long Id { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = "meters";
        public DateTime MeasuredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
