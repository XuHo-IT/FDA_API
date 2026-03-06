using FDAAPI.Domain.RelationalDb.Entities;

namespace FDAAPI.App.FeatG106_StationComponentUpdate
{
    public class StationComponentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? Id { get; set; }
        public StationComponent? Component { get; set; }

        public StationComponentResponse() { }

        public StationComponentResponse(bool success, string message, Guid? id = null, StationComponent? component = null)
        {
            Success = success;
            Message = message;
            Id = id;
            Component = component;
        }
    }
}
