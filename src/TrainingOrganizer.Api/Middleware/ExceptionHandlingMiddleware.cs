using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TrainingOrganizer.Application.Common.Exceptions;
using TrainingOrganizer.Domain.Exceptions;

namespace TrainingOrganizer.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        ProblemDetails problemDetails = exception switch
        {
            ApplicationValidationException validationException => CreateValidationProblemDetails(validationException),
            NotFoundException notFoundException => CreateNotFoundProblemDetails(notFoundException),
            ForbiddenException forbiddenException => CreateForbiddenProblemDetails(forbiddenException),
            DomainException domainException => CreateDomainProblemDetails(domainException),
            _ => CreateInternalServerErrorProblemDetails(exception)
        };

        if (problemDetails.Status >= 500)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "A handled exception occurred: {Message}", exception.Message);
        }

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
    }

    private static ProblemDetails CreateValidationProblemDetails(ApplicationValidationException exception)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "Validation Error",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred.",
            Extensions = { ["errors"] = exception.Errors }
        };
    }

    private static ProblemDetails CreateNotFoundProblemDetails(NotFoundException exception)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            Title = "Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = exception.Message
        };
    }

    private static ProblemDetails CreateForbiddenProblemDetails(ForbiddenException exception)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            Title = "Forbidden",
            Status = StatusCodes.Status403Forbidden,
            Detail = exception.Message
        };
    }

    private static ProblemDetails CreateDomainProblemDetails(DomainException exception)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.23",
            Title = "Unprocessable Entity",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = exception.Message
        };
    }

    private static ProblemDetails CreateInternalServerErrorProblemDetails(Exception exception)
    {
        return new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred. Please try again later."
        };
    }
}
