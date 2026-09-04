using Vargshala.Contracts.Common;

namespace Vargshala.Web.Common;

/// <summary>
/// Reusable table query state manager for all Vargshala pages.
/// Bundles PageNumber, PageSize, Search, and TableSortState into a unified PagedRequest.
/// </summary>
public class TableQueryState
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public TableSortState SortState { get; } = new();

    public PagedRequest ToPagedRequest()
    {
        return new PagedRequest
        {
            PageNumber = Math.Max(1, PageNumber),
            PageSize = PageSize,
            Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
            SortBy = SortState.IsSorted ? SortState.Column : null,
            SortDirection = SortState.IsSorted
                ? (SortState.Direction == SortDirection.Ascending ? "asc" : "desc")
                : null
        };
    }

    public void ResetPagination()
    {
        PageNumber = 1;
    }
}
