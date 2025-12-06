using Microsoft.AspNetCore.Http;

namespace ChatSR.Application.Shared.Errors;

public class Error
{
	private Error(int code, string message)
	{
		Code = code;
		Message = message;
	}

	public int Code { get; set; }
	public string Message { get; set; }

	public static Error NotFound(string message) => new(StatusCodes.Status404NotFound, message);
	public static Error Validation(string message) => new(StatusCodes.Status400BadRequest, message);
	public static Error Conflict(string message) => new(StatusCodes.Status409Conflict, message);
	public static Error InternalServerError(string message) => new(StatusCodes.Status500InternalServerError, message);
}
