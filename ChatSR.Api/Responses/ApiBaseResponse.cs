using ChatSR.Application.Shared.Errors;

namespace ChatSR.Api.Responses;

public abstract class ApiBaseResponse
{
	protected ApiBaseResponse(bool isSuccess, Error? error, DateTime timestamp)
	{
		if (isSuccess && error is not null)
			throw new ArgumentException("Successful response can't have an error.");

		if (!isSuccess && error is null)
			throw new ArgumentException("Failed response must have an error.");

		IsSuccess = isSuccess;
		Error = error;
		Timestamp = timestamp;
	}

	public bool IsSuccess { get; set; }
	public Error? Error { get; set; }
	public DateTime Timestamp { get; set; }

}
