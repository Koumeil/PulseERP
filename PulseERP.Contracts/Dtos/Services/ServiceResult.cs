namespace PulseERP.Contracts.Dtos.Services;

public class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public T? Data { get; }

    public bool IsFailure => !IsSuccess;

    private ServiceResult(T? value, bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
        Data = value;
    }

    public static ServiceResult<T> Success(T value) => new(value, true, null);

    public static ServiceResult<T> Failure(string error) => new(default, false, error);
}

public class ServiceResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    public bool IsFailure => !IsSuccess;

    private ServiceResult(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static ServiceResult Success() => new(true, null);
    public static ServiceResult Failure(string error) => new(false, error);
}
