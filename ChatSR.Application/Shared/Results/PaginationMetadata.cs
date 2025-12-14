namespace ChatSR.Application.Shared.Results;

public class PaginationMetadata
{
	public PaginationMetadata(int page, int pageSize, int totalCount)
	{

		if (page <= 0)
			throw new Exception("Page can't be less than or equal to 0.");

		if (pageSize <= 0)
			throw new Exception("Page Size can't be less than or equal to 0.");

		if (totalCount < 0)
			throw new Exception("Total Count can't be less than to 0.");

		Page = page;
		PageSize = pageSize;
		TotalCount = totalCount;
		TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
		HasPrevious = page > 1;
		HasNext = page < TotalPages;
	}

	public int Page { get; set; }
	public int PageSize { get; set; }
	public int TotalCount { get; set; }
	public int TotalPages { get; set; }
	public bool HasPrevious { get; set; }
	public bool HasNext { get; set; }
}
