using ChatSR.Application.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace ChatSR.Application.Dtos;

public record QueryParams
{
	private string? _searchTerm;

	[Range(1, int.MaxValue)]
	public int Page { get; init; } = PaginationConstants.DefaultPage;


	[Range(1, PaginationConstants.MaxPageSize)]
	public int PageSize { get; init; } = PaginationConstants.DefaultPageSize;


	public string? SearchTerm
	{
		get => _searchTerm;
		init => _searchTerm = string.IsNullOrWhiteSpace(value)
			? null
			: value.Trim();
	}
}
