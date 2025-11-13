using EasyBuy.Application.Common.Models;
using EasyBuy.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace EasyBuy.WebAPI.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = notFoundEx.Message;
                response.Errors = new List<string> { notFoundEx.Code };
                _logger.LogWarning(notFoundEx, "Not Found: {Message}", notFoundEx.Message);
                break;

            case Domain.Exceptions.ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = validationEx.Message;
                response.Errors = validationEx.Errors.SelectMany(e => e.Value).ToList();
                _logger.LogWarning(validationEx, "Validation Error: {Message}", validationEx.Message);
                break;

            case UnauthorizedException unauthorizedEx:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = unauthorizedEx.Message;
                response.Errors = new List<string> { unauthorizedEx.Code };
                _logger.LogWarning(unauthorizedEx, "Unauthorized: {Message}", unauthorizedEx.Message);
                break;

            case ForbiddenException forbiddenEx:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Message = forbiddenEx.Message;
                response.Errors = new List<string> { forbiddenEx.Code };
                _logger.LogWarning(forbiddenEx, "Forbidden: {Message}", forbiddenEx.Message);
                break;

            case ConflictException conflictEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Message = conflictEx.Message;
                response.Errors = new List<string> { conflictEx.Code };
                _logger.LogWarning(conflictEx, "Conflict: {Message}", conflictEx.Message);
                break;

            case BusinessRuleViolationException businessEx:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                response.Message = businessEx.Message;
                response.Errors = new List<string> { businessEx.Code };
                _logger.LogWarning(businessEx, "Business Rule Violation: {Message}", businessEx.Message);
                break;

            case ExternalServiceException serviceEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                response.StatusCode = (int)HttpStatusCode.BadGateway;
                response.Message = "An external service error occurred";
                response.Errors = new List<string> { serviceEx.Code };
                _logger.LogError(serviceEx, "External Service Error: {Message}", serviceEx.Message);
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = _env.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.";

                if (_env.IsDevelopment())
                {
                    response.Errors = new List<string>
                    {
                        exception.StackTrace ?? "No stack trace available"
                    };
                }

                _logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
