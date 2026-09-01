using System.Net;
using System.Text.Json;
using FluentValidation;
using Vargshala.Contracts.Common;
using Vargshala.Domain.Exceptions;

namespace Vargshala.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.FailureResponse(
                    "Validation failed.",
                    validationException.Errors.Select(e => e.ErrorMessage).ToList())),

            DomainException domainException => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.FailureResponse(domainException.Message)),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.FailureResponse("Unauthorized.")),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.FailureResponse("An unexpected error occurred."))
        };

        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
