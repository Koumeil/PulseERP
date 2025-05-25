namespace PulseERP.Contracts.Dtos.Services;

public class Result<T>
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public T? Data { get; }

    public bool IsFailure => !IsSuccess;

    private Result(T? value, bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
        Data = value;
    }

    public static Result<T> Success(T value) => new(value, true, null);

    public static Result<T> Failure(string error) => new(default, false, error);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}
