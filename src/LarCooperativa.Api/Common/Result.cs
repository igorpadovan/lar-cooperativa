namespace LarCooperativa.Api.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
}

public sealed record Error(ErrorType Type, string Code, string Message);

public class Result
{
    protected Result(Error? error) => Error = error;

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public static Result Success() => new(error: null);

    public static Result Failure(Error error) => new(error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T? value, Error? error) : base(error) => _value = value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Um resultado de falha não possui valor.");

    public static Result<T> Success(T value) => new(value, error: null);

    public static new Result<T> Failure(Error error) => new(default, error);
}
