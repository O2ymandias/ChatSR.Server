using ChatSR.Application.Shared.Errors;

namespace ChatSR.Api.Responses;

public class ApiResponse<T>
{
	private ApiResponse(bool isSuccess, T? data, Error? error, DateTime timestamp)
	{
		IsSuccess = isSuccess;
		Data = data;
		Error = error;
		Timestamp = timestamp;
	}

	public bool IsSuccess { get; set; }
	public T? Data { get; set; }
	public Error? Error { get; set; }
	public DateTime Timestamp { get; set; }

	public static ApiResponse<T> Success(T data) => new(true, data, default, DateTime.UtcNow);
	public static ApiResponse<T> Failure(Error error) => new(false, default, error, DateTime.UtcNow);
}
