namespace FDAAPI.App.Common.Features
{
    public sealed record UnitResponse(bool Success = true, string? Error = null) : IFeatureResponse
    {
        public static UnitResponse SuccessResult() => new(true, null);
        public static UnitResponse Failure(string error) => new(false, error);
    }
}

