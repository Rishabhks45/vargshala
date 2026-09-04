using FluentValidation;

namespace Vargshala.Contracts.Common;

#region Request
public class PagedRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;
    private int _pageSize = DefaultPageSize;
    private int _pageNumber = 1;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
    }

    public string? Search { get; set; }
    public string? SortBy { get; set; }

    private string _sortDirection = "desc";
    public string? SortDirection
    {
        get => _sortDirection;
        set => _sortDirection = string.IsNullOrWhiteSpace(value) ? "desc" : value;
    }

    public bool IsAscending => string.Equals(SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
}

public class PagedRequestValidator : AbstractValidator<PagedRequest>
{
    public PagedRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.SortDirection)
            .Must(dir => string.IsNullOrWhiteSpace(dir)
                      || string.Equals(dir, "asc", StringComparison.OrdinalIgnoreCase)
                      || string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be 'asc' or 'desc'.");
    }
}
#endregion

#region Response
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalRecords { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalRecords / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResponse<T> Create(List<T> items, int totalRecords, int pageNumber, int pageSize) => new()
    {
        Items = items,
        TotalRecords = totalRecords,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}
#endregion
