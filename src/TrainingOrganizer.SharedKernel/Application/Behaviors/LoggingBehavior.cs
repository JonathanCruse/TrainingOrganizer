using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using TrainingOrganizer.SharedKernel.Application.Interfaces;

namespace TrainingOrganizer.SharedKernel.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.MemberId?.ToString() ?? "Anonymous";

        _logger.LogInformation(
            "Handling {RequestName} for user {UserId}",
            requestName, userId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} for user {UserId} in {ElapsedMilliseconds}ms",
                requestName, userId, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error handling {RequestName} for user {UserId} after {ElapsedMilliseconds}ms",
                requestName, userId, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
