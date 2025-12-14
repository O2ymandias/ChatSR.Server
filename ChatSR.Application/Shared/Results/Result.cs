using ChatSR.Application.Shared.Errors;

namespace ChatSR.Application.Shared.Results;

public class Result<T> : BaseResult
{
	private Result(bool isSuccess, Error? error, T? value)
		: base(isSuccess, error)
	{
		Value = value;
	}

	public T? Value { get; set; }

	public static Result<T> Success(T value) => new(true, null, value);
	public static Result<T> Failure(Error error) => new(false, error, default);
}
