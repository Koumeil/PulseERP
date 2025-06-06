using Microsoft.AspNetCore.Mvc;

namespace PulseERP.API.Contracts;

public record ApiResponse<T>(
    bool Success,
    T? Data = default,
    string? Message = null,
    ProblemDetails? Error = null
);

public record ApiResponse(bool Success, string? Message = null, ProblemDetails? Error = null);
