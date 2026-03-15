using FluentValidation.Results;

namespace TrainingOrganizer.Application.Common.Exceptions;

public sealed class ApplicationValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ApplicationValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occurred.")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
