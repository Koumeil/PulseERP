using System;

namespace PulseERP.Application.Common;

public class ServiceResult<T>
{
    public bool Success { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ServiceResult() { }

    public static ServiceResult<T> Ok(T data) =>
        new ServiceResult<T> { Success = true, Data = data };

    public static ServiceResult<T> Fail(string errorCode, string errorMessage) =>
        new ServiceResult<T>
        {
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        };
}

public class ServiceResult
{
    public bool Success { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ServiceResult() { }

    public static ServiceResult Ok() => new ServiceResult { Success = true };

    public static ServiceResult Fail(string errorCode, string errorMessage) =>
        new ServiceResult
        {
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        };
}
