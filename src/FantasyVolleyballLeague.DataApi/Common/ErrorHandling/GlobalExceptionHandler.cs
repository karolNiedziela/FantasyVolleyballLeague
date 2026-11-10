using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FantasyVolleyballLeague.DataApi.Common.ErrorHandling
{
    internal partial class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetails;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IProblemDetailsService problemDetails,
            IHostEnvironment environment)
        {
            _logger = logger;
            _problemDetails = problemDetails;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            LogUnhandledException(exception, httpContext.TraceIdentifier);

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = _environment.IsDevelopment() ? exception.Message : null,
            };

            await _problemDetails.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
            });

            return true;
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception. TraceId: {TraceId}")]
        private partial void LogUnhandledException(Exception exception, string traceId);
    }
}
