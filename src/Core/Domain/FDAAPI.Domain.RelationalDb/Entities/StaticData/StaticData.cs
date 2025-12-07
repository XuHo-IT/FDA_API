namespace FDAAPI.App.Common.Models.StaticData;

public class DanangCenterDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double LatitudeDelta { get; set; }
    public double LongitudeDelta { get; set; }
}

public enum SensorStatus
{
    Safe,
    Warning,
    Danger
}

public class SensorDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int WaterLevel { get; set; }
    public int MaxLevel { get; set; }
    public SensorStatus Status { get; set; }
    public string? StatusText { get; set; }
    public string? LastUpdate { get; set; }
    public int Temperature { get; set; }
    public int Humidity { get; set; }
}

public class FloodZoneCoordinateDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class FloodZoneDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public SensorStatus Status { get; set; }
    public List<FloodZoneCoordinateDto> Coordinates { get; set; } = new List<FloodZoneCoordinateDto>();
}

public static class RawData
{
    public static readonly DanangCenterDto DANANG_CENTER = new DanangCenterDto
    {
        Latitude = 16.0544,
        Longitude = 108.2022,
        LatitudeDelta = 0.15,
        LongitudeDelta = 0.15,
    };

    public static readonly List<SensorDto> MOCK_SENSORS = new List<SensorDto>
    {
        new SensorDto
        {
            Id = "S01",
            Name = "Sông Hàn",
            Location = "Cầu Trần Thị Lý",
            Latitude = 16.0678,
            Longitude = 108.2229,
            WaterLevel = 35,
            MaxLevel = 50,
            Status = SensorStatus.Warning,
            StatusText = "Cảnh báo",
            LastUpdate = "2 phút trước",
            Temperature = 28,
            Humidity = 85,
        },
        new SensorDto
        {
            Id = "S02",
            Name = "Cầu Rồng",
            Location = "Quận Sơn Trà",
            Latitude = 16.0605,
            Longitude = 108.2273,
            WaterLevel = 65,
            MaxLevel = 50,
            Status = SensorStatus.Danger,
            StatusText = "Nguy hiểm",
            LastUpdate = "1 phút trước",
            Temperature = 29,
            Humidity = 88,
        },
        new SensorDto
        {
            Id = "S03",
            Name = "Hải Châu",
            Location = "Trung tâm TP",
            Latitude = 16.0471,
            Longitude = 108.2091,
            WaterLevel = 25,
            MaxLevel = 50,
            Status = SensorStatus.Safe,
            StatusText = "An toàn",
            LastUpdate = "5 phút trước",
            Temperature = 27,
            Humidity = 75,
        },
        new SensorDto
        {
            Id = "S04",
            Name = "Bán đảo",
            Location = "Sơn Trà",
            Latitude = 16.0864,
            Longitude = 108.2440,
            WaterLevel = 45,
            MaxLevel = 50,
            Status = SensorStatus.Warning,
            StatusText = "Cảnh báo",
            LastUpdate = "3 phút trước",
            Temperature = 26,
            Humidity = 80,
        },
        new SensorDto
        {
            Id = "S05",
            Name = "Thanh Khê",
            Location = "Quận Thanh Khê",
            Latitude = 16.0673,
            Longitude = 108.1926,
            WaterLevel = 28,
            MaxLevel = 50,
            Status = SensorStatus.Safe,
            StatusText = "An toàn",
            LastUpdate = "4 phút trước",
            Temperature = 27,
            Humidity = 78,
        },
    };

    public static readonly List<FloodZoneDto> FLOOD_ZONES = new List<FloodZoneDto>
    {
        new FloodZoneDto
        {
            Id = "zone-1",
            Name = "Khu vực Hải Châu",
            Status = SensorStatus.Danger,
            Coordinates = new List<FloodZoneCoordinateDto>
            {
                new FloodZoneCoordinateDto { Latitude = 16.0471, Longitude = 108.2091 },
                new FloodZoneCoordinateDto { Latitude = 16.0471, Longitude = 108.2180 },
                new FloodZoneCoordinateDto { Latitude = 16.0400, Longitude = 108.2180 },
                new FloodZoneCoordinateDto { Latitude = 16.0400, Longitude = 108.2091 },
            },
        },
        new FloodZoneDto
        {
            Id = "zone-2",
            Name = "Khu vực Cầu Rồng",
            Status = SensorStatus.Warning,
            Coordinates = new List<FloodZoneCoordinateDto>
            {
                new FloodZoneCoordinateDto { Latitude = 16.0625, Longitude = 108.2220 },
                new FloodZoneCoordinateDto { Latitude = 16.0625, Longitude = 108.2310 },
                new FloodZoneCoordinateDto { Latitude = 16.0560, Longitude = 108.2310 },
                new FloodZoneCoordinateDto { Latitude = 16.0560, Longitude = 108.2220 },
            },
        },
    };
}
