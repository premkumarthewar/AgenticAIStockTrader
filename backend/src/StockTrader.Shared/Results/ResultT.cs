namespace StockTrader.Shared.Results;

public sealed class Result<T> : Result
{
    private readonly T? value;

    private Result(
        T? value,
        bool isSuccess,
        Error error)
        : base(isSuccess, error)
    {
        this.value = value;
    }

    public T Value =>
        IsSuccess
            ? value!
            : throw new InvalidOperationException(
                "Cannot access the value of a failed result.");

    public static Result<T> Success(T value)
        => new(value, true, Error.None);

    public static new Result<T> Failure(Error error)
        => new(default, false, error);
}