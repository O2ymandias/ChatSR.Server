using ChatSR.Application.Shared.Errors;

namespace ChatSR.Application.Shared.Results;

public class PagedResult<T> : ResultBase
{
	private PagedResult(bool isSuccess, Error? error, IReadOnlyList<T>? items, PaginationMetadata? pagination)
		: base(isSuccess, error)
	{
		Items = items;
		Pagination = pagination;
	}

	public IReadOnlyList<T>? Items { get; set; }
	public PaginationMetadata? Pagination { get; set; }

	public static PagedResult<T> Success(IReadOnlyList<T> items, PaginationMetadata pagination) =>
		new(true, default, items, pagination);

	public static PagedResult<T> Failure(Error error) =>
		new(false, error, default, default);
}
