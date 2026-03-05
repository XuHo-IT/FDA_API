namespace FDAAPI.App.Common.Models.Common
{
    public class Result<TValue>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public TValue Value { get; }
        public string Error { get; }

        protected Result(TValue value, bool isSuccess, string error)
        {
            Value = value;
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result<TValue> Success(TValue value) => new(value, true, string.Empty);
        public static Result<TValue> Failure(string error) => new(default!, false, error);
    }
}

