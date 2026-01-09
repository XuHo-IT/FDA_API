namespace FDAAPI.App.FeatG28_GetMapPreferences
{
    public class MapLayerSettings
    {
        public string BaseMap { get; set; } = "standard";
        public OverlaySettings Overlays { get; set; } = new();
        public OpacitySettings Opacity { get; set; } = new();
    }

    public class OverlaySettings
    {
        public bool Flood { get; set; } = true;
        public bool Traffic { get; set; } = false;
        public bool Weather { get; set; } = false;
    }

    public class OpacitySettings
    {
        public int Flood { get; set; } = 80;
        public int Weather { get; set; } = 70;
    }
}
