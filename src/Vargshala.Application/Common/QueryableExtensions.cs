using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Vargshala.Contracts.Common;

namespace Vargshala.Application.Common;

/// <summary>
/// Reusable server-side querying extensions for IQueryable.
/// Applies Search → Sort → CountAsync → Skip/Take → ToListAsync — all at database level.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies server-side search, sort, count, and pagination to an IQueryable pipeline.
    /// The query remains IQueryable until final execution. No in-memory operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The base IQueryable (should already have tenant/soft-delete filters applied).</param>
    /// <param name="request">The paged request with PageNumber, PageSize, Search, SortBy, SortDirection.</param>
    /// <param name="searchPredicate">
    /// Optional function that takes the search term and returns a Where predicate.
    /// Example: term => u => u.FirstName.Contains(term) || u.Email.Contains(term)
    /// </param>
    /// <param name="sortMappings">
    /// Dictionary mapping allowed sort field names (lowercase) to entity property expressions.
    /// Example: { "firstname" => u => u.FirstName, "createdat" => u => u.CreatedAt }
    /// </param>
    /// <param name="defaultSortExpression">
    /// The default sort expression when SortBy is null or not in whitelist.
    /// Typically: e => e.CreatedAt
    /// </param>
    /// <param name="defaultAscending">
    /// Whether the default sort is ascending. Defaults to false (DESC) for CreatedAt.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of (paginated items list, total records count).</returns>
    public static async Task<(List<T> Items, int TotalRecords)> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedRequest request,
        Func<string, Expression<Func<T, bool>>>? searchPredicate = null,
        Dictionary<string, Expression<Func<T, object>>>? sortMappings = null,
        Expression<Func<T, object>>? defaultSortExpression = null,
        bool defaultAscending = false,
        CancellationToken cancellationToken = default)
    {
        // 1. Apply Search (database-level WHERE)
        if (!string.IsNullOrWhiteSpace(request.Search) && searchPredicate != null)
        {
            var predicate = searchPredicate(request.Search.Trim());
            query = query.Where(predicate);
        }

        // 2. Apply Sorting (database-level ORDER BY)
        query = ApplySorting(query, request.SortBy, request.IsAscending,
            sortMappings, defaultSortExpression, defaultAscending);

        // 3. Count total records AFTER search/filter, BEFORE pagination
        var totalRecords = await query.CountAsync(cancellationToken);

        // 4. Apply Pagination (database-level OFFSET/FETCH)
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalRecords);
    }

    /// <summary>
    /// Applies server-side search, sort, count, and pagination, then projects to DTOs.
    /// Uses Select() before ToListAsync() so only needed columns are fetched from the database.
    /// </summary>
    public static async Task<(List<TDto> Items, int TotalRecords)> ToPagedResultAsync<T, TDto>(
        this IQueryable<T> query,
        PagedRequest request,
        Expression<Func<T, TDto>> selector,
        Func<string, Expression<Func<T, bool>>>? searchPredicate = null,
        Dictionary<string, Expression<Func<T, object>>>? sortMappings = null,
        Expression<Func<T, object>>? defaultSortExpression = null,
        bool defaultAscending = false,
        CancellationToken cancellationToken = default)
    {
        // 1. Apply Search
        if (!string.IsNullOrWhiteSpace(request.Search) && searchPredicate != null)
        {
            var predicate = searchPredicate(request.Search.Trim());
            query = query.Where(predicate);
        }

        // 2. Apply Sorting
        query = ApplySorting(query, request.SortBy, request.IsAscending,
            sortMappings, defaultSortExpression, defaultAscending);

        // 3. Count total records
        var totalRecords = await query.CountAsync(cancellationToken);

        // 4. Apply Pagination + Projection
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return (items, totalRecords);
    }

    /// <summary>
    /// Applies safe dynamic sorting using a whitelist of allowed sort field mappings.
    /// Falls back to the default sort expression if SortBy is null or not in the whitelist.
    /// Never concatenates user input into SQL.
    /// </summary>
    private static IQueryable<T> ApplySorting<T>(
        IQueryable<T> query,
        string? sortBy,
        bool isAscending,
        Dictionary<string, Expression<Func<T, object>>>? sortMappings,
        Expression<Func<T, object>>? defaultSortExpression,
        bool defaultAscending)
    {
        Expression<Func<T, object>>? sortExpression = null;
        var ascending = isAscending;

        // Try to resolve from whitelist
        if (!string.IsNullOrWhiteSpace(sortBy) && sortMappings != null)
        {
            var key = sortBy.Trim().ToLowerInvariant();
            if (sortMappings.TryGetValue(key, out var mappedExpression))
            {
                sortExpression = mappedExpression;
            }
        }

        // Fallback to default
        if (sortExpression == null)
        {
            if (defaultSortExpression != null)
            {
                sortExpression = defaultSortExpression;
                ascending = defaultAscending;
            }
            else
            {
                // No sort at all — return as-is (EF will use DB default ordering)
                return query;
            }
        }

        return ascending
            ? query.OrderBy(sortExpression)
            : query.OrderByDescending(sortExpression);
    }
}
