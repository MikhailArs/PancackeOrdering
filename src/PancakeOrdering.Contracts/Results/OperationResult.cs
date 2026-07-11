namespace PancakeOrdering.Contracts.Results;

public class OperationResult
{
    protected OperationResult(bool isSuccess, OperationErrorCode? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public OperationErrorCode? Error { get; }

    public static OperationResult Success()
        => new(true, null);

    public static OperationResult Failure(OperationErrorCode error)
        => new(false, error);
}

public sealed class OperationResult<T> : OperationResult
{
    private OperationResult(T value)
        : base(true, null)
    {
        Value = value;
    }

    private OperationResult(OperationErrorCode error)
        : base(false, error)
    {
    }

    public T? Value { get; }

    public static OperationResult<T> Success(T value)
        => new(value);

    public static new OperationResult<T> Failure(OperationErrorCode error)
        => new(error);
}
