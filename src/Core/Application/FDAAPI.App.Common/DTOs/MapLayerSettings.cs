using System.Collections.Generic;

namespace FDAAPI.App.Common.DTOs
{
    /// <summary>
    /// Map layer settings DTO for user preferences
    /// </summary>
    public class MapLayerSettings
    {
        public string BaseMap { get; set; } = "standard";
        public OverlaySettings Overlays { get; set; } = new();
        public OpacitySettings Opacity { get; set; } = new();
    }

    /// <summary>
    /// Overlay settings for map layers
    /// </summary>
    public class OverlaySettings
    {
        public bool Flood { get; set; } = true;
        public bool Traffic { get; set; } = false;
        public bool Weather { get; set; } = false;
    }

    /// <summary>
    /// Opacity settings for map layers
    /// </summary>
    public class OpacitySettings
    {
        public int Flood { get; set; } = 80;
        public int Weather { get; set; } = 70;
    }
}
