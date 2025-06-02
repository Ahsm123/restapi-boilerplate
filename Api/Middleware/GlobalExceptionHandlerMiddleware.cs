using Api.Exceptions;
using Api.Exceptions.TokenExceptions;
using System.Net;
using System.Text.Json;

namespace Api.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                UserNotFoundException => (HttpStatusCode.NotFound, exception.Message),
                EmailAlreadyTakenException => (HttpStatusCode.BadRequest, exception.Message),
                InvalidCredentialsException => (HttpStatusCode.Unauthorized, exception.Message),
                EmailNotVerifiedException => (HttpStatusCode.Unauthorized, exception.Message),
                WeakPasswordException => (HttpStatusCode.BadRequest, exception.Message),
                TokenNotFoundException => (HttpStatusCode.Unauthorized, "Invalid or expired token"),
                TokenExpiredException => (HttpStatusCode.Unauthorized, "Token has expired"),
                TokenBlacklistedException => (HttpStatusCode.Unauthorized, "Token has been revoked"),
                InvalidTokenException => (HttpStatusCode.Unauthorized, "Invalid token"),
                ValidationException => (HttpStatusCode.BadRequest, exception.Message),
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
                _ => (HttpStatusCode.InternalServerError, "An error occurred while processing your request")
            };

            response.StatusCode = (int)statusCode;

            var errorResponse = new
            {
                message,
                statusCode = (int)statusCode,
                timestamp = DateTime.UtcNow
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(jsonResponse);
        }
    }
}
