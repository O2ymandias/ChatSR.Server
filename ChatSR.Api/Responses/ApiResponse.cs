using ChatSR.Application.Shared.Errors;

namespace ChatSR.Api.Responses;

public class ApiResponse : ApiBaseResponse
{
	private ApiResponse(bool isSuccess, Error? error, DateTime timestamp)
		: base(isSuccess, error, timestamp)
	{
	}

	public static ApiResponse Success() => new(true, null, DateTime.UtcNow);
	public static ApiResponse Failure(Error error) => new(false, error, DateTime.UtcNow);
}

public class ApiResponse<T> : ApiBaseResponse
{
	private ApiResponse(bool isSuccess, T? data, Error? error, DateTime timestamp)
		: base(isSuccess, error, timestamp)
	{
		Data = data;
	}

	public T? Data { get; set; }

	public static ApiResponse<T> Success(T data) => new(true, data, default, DateTime.UtcNow);
	public static ApiResponse<T> Failure(Error error) => new(false, default, error, DateTime.UtcNow);
}
