using FDAAPI.Domain.RelationalDb.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace FDAAPI.Domain.RelationalDb
{
    public class SensorReading
    {
        public Guid Id { get; set; }
        public Guid StationId { get; set; }
        public double Value { get; set; }
        public double Distance { get; set; }
        public double SensorHeight { get; set; }
        public string Unit { get; set; } = "cm";
        public int Status { get; set; }
        public DateTime MeasuredAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        [ForeignKey(nameof(StationId))]
        public virtual Station? Station { get; set; }

    }
}