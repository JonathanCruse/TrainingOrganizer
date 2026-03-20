namespace TrainingOrganizer.SharedKernel.Application.Models;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(string code, string message) => new(false, new Error(code, message));

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
    public static Result<T> Failure<T>(string code, string message) => Result<T>.Failure(new Error(code, message));
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    private Result(bool isSuccess, Error error, T? value) : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new(true, Error.None, value);
    public new static Result<T> Failure(Error error) => new(false, error, default);
    public new static Result<T> Failure(string code, string message) => new(false, new Error(code, message), default);
}
