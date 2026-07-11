namespace PancakeOrdering.Core.Common.Results
{
    public class Result
    {
        protected Result(bool isSuccess, ErrorCode? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }

        public ErrorCode? Error { get; }

        public static Result Success()
            => new(true, null);

        public static Result Failure(ErrorCode error)
            => new(false, error);

        public static Result<T> Success<T>(T value)
            => new(value);

        public static Result<T> Failure<T>(ErrorCode error)
            => new(error);
    }

    public sealed class Result<T> : Result
    {
        internal Result(T value)
            : base(true, null)
        {
            Value = value;
        }

        internal Result(ErrorCode error)
            : base(false, error)
        {
        }

        public T? Value { get; }

        public bool HasValue => IsSuccess;
    }
}
