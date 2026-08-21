using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Api.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception,
                    "unhandled exception on {Method} {Path}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);

            var (statusCode, title) = exception switch
            {
                ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Invalid value supplied"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = _hostEnvironment.IsDevelopment()
                    ? exception.Message
                    : null,
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                 problemDetails,
                 cancellationToken);

            return true;


        }
    }
}
