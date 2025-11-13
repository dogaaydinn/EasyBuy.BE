namespace EasyBuy.Application.Common.Models;

/// <summary>
/// Represents the result of an operation
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string Message { get; protected set; } = string.Empty;
    public List<string> Errors { get; protected set; } = new();

    protected Result(bool isSuccess, string message, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors ?? new List<string>();
    }

    public static Result Success(string message = "Operation completed successfully")
        => new(true, message);

    public static Result Failure(string message, List<string>? errors = null)
        => new(false, message, errors);

    public static Result Failure(string message, string error)
        => new(false, message, new List<string> { error });
}

/// <summary>
/// Represents the result of an operation with a return value
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; private set; }

    private Result(bool isSuccess, string message, T? data = default, List<string>? errors = null)
        : base(isSuccess, message, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string message = "Operation completed successfully")
        => new(true, message, data);

    public new static Result<T> Failure(string message, List<string>? errors = null)
        => new(false, message, default, errors);

    public new static Result<T> Failure(string message, string error)
        => new(false, message, default, new List<string> { error });
}
