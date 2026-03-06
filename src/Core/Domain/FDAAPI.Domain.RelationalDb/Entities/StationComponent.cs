using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FDAAPI.Domain.RelationalDb.Entities
{
    /// <summary>
    /// Component entity representing hardware components within a station
    /// </summary>
    public class StationComponent : EntityWithId<Guid>, ICreatedEntity<Guid>, IUpdatedEntity<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? ComponentType { get; set; } // esp32, srt04, temperature_sensor, battery, speaker, gsm_module, solar_panel, rain_sensor

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [MaxLength(50)]
        public string? FirmwareVersion { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; } = "active"; // active, inactive, faulty

        public DateTimeOffset? InstalledAt { get; set; }

        public DateTimeOffset? LastMaintenanceAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(StationId))]
        public virtual Station? Station { get; set; }
    }

    /// <summary>
    /// Component type constants
    /// </summary>
    public static class StationComponentTypes
    {
        public const string ESP32 = "esp32";
        public const string SRT04 = "srt04";
        public const string TemperatureSensor = "temperature_sensor";
        public const string Battery = "battery";
        public const string Speaker = "speaker";
        public const string GsmModule = "gsm_module";
        public const string SolarPanel = "solar_panel";
        public const string RainSensor = "rain_sensor";

        public static readonly string[] All = new[]
        {
            ESP32, SRT04, TemperatureSensor, Battery, Speaker, GsmModule, SolarPanel, RainSensor
        };
    }

    /// <summary>
    /// Component status constants
    /// </summary>
    public static class StationComponentStatuses
    {
        public const string Active = "active";
        public const string Inactive = "inactive";
        public const string Faulty = "faulty";

        public static readonly string[] All = new[] { Active, Inactive, Faulty };
    }
}
