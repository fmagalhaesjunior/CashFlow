namespace CashFlow.BuildingBlocks.Application.Results;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public string? Error { get; protected set; }

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    public static Result<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    public static new Result<T> Failure(string error) => new()
    {
        IsSuccess = false,
        Error = error
    };
}