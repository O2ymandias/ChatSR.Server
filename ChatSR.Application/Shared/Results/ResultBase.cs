using ChatSR.Application.Shared.Errors;

namespace ChatSR.Application.Shared.Results;

public abstract class ResultBase
{
	protected ResultBase(bool isSuccess, Error? error)
	{
		if (isSuccess && error is not null)
			throw new ArgumentException("Successful result can't have an error.");

		if (!isSuccess && error is null)
			throw new ArgumentException("Failed result must have an error.");

		IsSuccess = isSuccess;
		Error = error;
	}

	public bool IsSuccess { get; set; }
	public Error? Error { get; set; }
}
