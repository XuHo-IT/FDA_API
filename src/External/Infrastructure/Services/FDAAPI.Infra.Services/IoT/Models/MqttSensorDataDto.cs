using System.Text.Json.Serialization;

public class MqttSensorDataDto
{
    [JsonPropertyName("station_id")]
    public string StationId { get; set; } = string.Empty;

    [JsonPropertyName("water_level")]
    public double WaterLevel { get; set; }

    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [JsonPropertyName("sensor_height")]
    public double SensorHeight { get; set; }

    [JsonPropertyName("measured_at")]
    public string? MeasuredAt { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }
}