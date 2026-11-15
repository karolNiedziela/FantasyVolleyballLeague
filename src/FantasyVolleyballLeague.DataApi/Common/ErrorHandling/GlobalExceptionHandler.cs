using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

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
            var problem = exception switch
            {
                BadHttpRequestException badRequest => CreateBadRequestProblem(httpContext, badRequest),
                _ => CreateInternalServerErrorProblem(httpContext, exception),
            };

            httpContext.Response.StatusCode = problem.Status!.Value;

            await _problemDetails.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
            });

            return true;
        }

        private ProblemDetails CreateBadRequestProblem(HttpContext httpContext, BadHttpRequestException exception)
        {
            LogBadRequest(exception.Message, httpContext.TraceIdentifier);

            return new ProblemDetails
            {
                Status = exception.StatusCode,
                Title = ReasonPhrases.GetReasonPhrase(exception.StatusCode),
                Detail = exception.Message,
            };
        }

        private ProblemDetails CreateInternalServerErrorProblem(HttpContext httpContext, Exception exception)
        {
            LogUnhandledException(exception, httpContext.TraceIdentifier);

            return new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = _environment.IsDevelopment() ? exception.Message : null,
            };
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception. TraceId: {TraceId}")]
        private partial void LogUnhandledException(Exception exception, string traceId);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Bad request: {Reason}. TraceId: {TraceId}")]
        private partial void LogBadRequest(string reason, string traceId);
    }
}
