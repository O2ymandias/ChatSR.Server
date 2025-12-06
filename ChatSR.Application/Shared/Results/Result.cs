using ChatSR.Application.Shared.Errors;

namespace ChatSR.Application.Shared.Results;

public class Result<T>
{
	private Result(bool isSuccess, Error? error, T? value)
	{
		if (isSuccess && error is not null)
			throw new ArgumentException("Successful result can't have an error.");

		if (!isSuccess && error is null)
			throw new ArgumentException("Failed result must have an error.");

		IsSuccess = isSuccess;
		Error = error;
		Value = value;
	}

	public bool IsSuccess { get; set; }
	public Error? Error { get; set; }
	public T? Value { get; set; }

	public static Result<T> Success(T value) => new(true, null, value);
	public static Result<T> Failure(Error error) => new(false, error, default);
}
