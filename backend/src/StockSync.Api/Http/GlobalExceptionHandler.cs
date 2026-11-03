using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace StockSync.Api.Http;

internal sealed partial class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, code, detail) = Classify(exception, cancellationToken);

        if (status >= StatusCodes.Status500InternalServerError)
        {
            LogUnhandled(logger, exception, httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            LogHandled(logger, httpContext.Request.Method, httpContext.Request.Path, code);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Detail = detail,
                Extensions = { ["code"] = code },
            },
        });
    }

    private static (int Status, string Code, string Detail) Classify(Exception exception, CancellationToken ct) => exception switch
    {
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency",
            "The data was changed by another request at the same time. Please reload and try again."),
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } } =>
            (StatusCodes.Status409Conflict, "Duplicate", "A record with the same unique value already exists."),
        DbUpdateException { InnerException: PostgresException { SqlState: PostgresErrorCodes.CheckViolation } } =>
            (StatusCodes.Status409Conflict, "ConstraintViolation", "The change would break a data rule."),
        BadHttpRequestException bad => (bad.StatusCode, "BadRequest", "The request could not be read."),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication is required."),
        OperationCanceledException when ct.IsCancellationRequested => (ClientClosedRequest, "Cancelled", "The request was cancelled."),
        _ => (StatusCodes.Status500InternalServerError, "ServerError", "An unexpected error occurred."),
    };

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception for {Method} {Path}")]
    private static partial void LogUnhandled(ILogger logger, Exception exception, string method, PathString path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Request {Method} {Path} failed with {Code}")]
    private static partial void LogHandled(ILogger logger, string method, PathString path, string code);
}
