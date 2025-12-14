using ChatSR.Application.Shared.Errors;
using ChatSR.Application.Shared.Results;

namespace ChatSR.Api.Responses;

public class PagedApiResponse<T>
{
	private PagedApiResponse(bool isSuccess, IReadOnlyList<T>? items, PaginationMetadata? pagination, Error? error, DateTime timestamp)
	{
		IsSuccess = isSuccess;
		Items = items;
		Pagination = pagination;
		Error = error;
		Timestamp = timestamp;
	}

	public bool IsSuccess { get; set; }
	public IReadOnlyList<T>? Items { get; set; }
	public PaginationMetadata? Pagination { get; set; }
	public Error? Error { get; set; }
	public DateTime Timestamp { get; set; }

	public static PagedApiResponse<T> Success(IReadOnlyList<T> items, PaginationMetadata pagination) =>
		new(true, items, pagination, default, DateTime.UtcNow);
	public static PagedApiResponse<T> Failure(Error error) =>
		new(false, default, default, error, DateTime.UtcNow);
}
