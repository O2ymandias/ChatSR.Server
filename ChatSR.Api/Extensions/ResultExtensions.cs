using ChatSR.Api.Responses;
using ChatSR.Application.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace ChatSR.Api.Extensions;

public static class ResultExtensions
{

	public static IActionResult ToActionResult(this Result result)
	{
		if (result.IsSuccess)
		{
			var successResponse = ApiResponse.Success();
			return new OkObjectResult(successResponse);
		}

		var code = result.Error!.Code;
		var errorResponse = ApiResponse.Failure(result.Error);
		return code switch
		{
			StatusCodes.Status400BadRequest => new BadRequestObjectResult(errorResponse),
			StatusCodes.Status404NotFound => new NotFoundObjectResult(errorResponse),
			StatusCodes.Status409Conflict => new BadRequestObjectResult(errorResponse),
			_ => new ObjectResult(errorResponse) { StatusCode = code }
		};
	}

	public static IActionResult ToActionResult<T>(this Result<T> result)
	{
		if (result.IsSuccess)
		{
			var successResponse = ApiResponse<T>.Success(result.Value!);
			return new OkObjectResult(successResponse);
		}

		var code = result.Error!.Code;
		var errorResponse = ApiResponse<T>.Failure(result.Error);
		return code switch
		{
			StatusCodes.Status400BadRequest => new BadRequestObjectResult(errorResponse),
			StatusCodes.Status404NotFound => new NotFoundObjectResult(errorResponse),
			StatusCodes.Status409Conflict => new BadRequestObjectResult(errorResponse),
			_ => new ObjectResult(errorResponse) { StatusCode = code }
		};
	}

	public static IActionResult ToActionResult<T>(this PagedResult<T> result)
	{
		if (result.IsSuccess)
		{
			var items = result.Items!;
			var pagination = result.Pagination!;
			return new OkObjectResult(PagedApiResponse<T>.Success(items, pagination));
		}

		var code = result.Error!.Code;
		var errorResponse = PagedApiResponse<T>.Failure(result.Error);

		return code switch
		{
			StatusCodes.Status400BadRequest => new BadRequestObjectResult(errorResponse),
			_ => new ObjectResult(errorResponse) { StatusCode = code }
		};
	}
}
