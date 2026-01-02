using ChatSR.Application.Shared.Errors;
using ChatSR.Application.Shared.Results;

namespace ChatSR.Api.Responses;

public class PagedApiResponse<T> : ApiBaseResponse
{
	private PagedApiResponse(bool isSuccess, IReadOnlyList<T>? items, PaginationMetadata? pagination, Error? error, DateTime timestamp)
		: base(isSuccess, error, timestamp)
	{
		Items = items;
		Pagination = pagination;
	}

	public IReadOnlyList<T>? Items { get; set; }
	public PaginationMetadata? Pagination { get; set; }

	public static PagedApiResponse<T> Success(IReadOnlyList<T> items, PaginationMetadata pagination) =>
		new(true, items, pagination, default, DateTime.UtcNow);
	public static PagedApiResponse<T> Failure(Error error) =>
		new(false, default, default, error, DateTime.UtcNow);
}
