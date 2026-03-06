using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.FeatG108_StationComponentList
{
    public class StationComponentListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<StationComponent>? Components { get; set; }

        public StationComponentListResponse() { }

        public StationComponentListResponse(bool success, string message, IEnumerable<StationComponent>? components = null)
        {
            Success = success;
            Message = message;
            Components = components;
        }
    }
}
