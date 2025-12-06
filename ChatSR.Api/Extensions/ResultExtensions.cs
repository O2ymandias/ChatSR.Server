using ChatSR.Api.Responses;
using ChatSR.Application.Dtos.AuthDtos;
using ChatSR.Application.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace ChatSR.Api.Extensions;

public static class ResultExtensions
{
	public static IActionResult ToActionResult<T>(this Result<T> result)
	{
		if (result.IsSuccess)
		{
			var successResponse = ApiResponse<T>.Success(result.Value!);
			return new OkObjectResult(successResponse);
		}

		var code = result.Error!.Code;
		var errorResponse = ApiResponse<AuthResponse>.Failure(result.Error);
		return code switch
		{
			StatusCodes.Status400BadRequest => new BadRequestObjectResult(errorResponse),
			StatusCodes.Status404NotFound => new NotFoundObjectResult(errorResponse),
			StatusCodes.Status409Conflict => new BadRequestObjectResult(errorResponse),
			_ => new ObjectResult(errorResponse) { StatusCode = code }
		};
	}
}
