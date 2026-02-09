namespace WorkforceAPI.Models.DTOs;

/// <summary>
/// Generic paginated result wrapper for API responses
/// </summary>
/// <typeparam name="T">The type of items in the paginated result</typeparam>
/// <remarks>
/// This generic class provides a standardized format for paginated API responses.
/// It includes:
/// - The data items for the current page
/// - Pagination metadata (page number, page size, total count)
/// - Helper properties for pagination UI (total pages, has previous/next page)
/// 
/// Usage example:
/// <code>
/// var result = new PagedResult&lt;EmployeeListDto&gt;
/// {
///     Data = employees,
///     Page = 1,
///     PageSize = 10,
///     TotalCount = 100
/// };
/// // result.TotalPages = 10
/// // result.HasPreviousPage = false
/// // result.HasNextPage = true
/// </code>
/// 
/// This allows frontend applications to:
/// - Display pagination controls
/// - Calculate total pages
/// - Enable/disable previous/next buttons
/// - Show "Page X of Y" information
/// </remarks>
public class PagedResult<T>
{
    /// <summary>
    /// The data items for the current page
    /// </summary>
    public IEnumerable<T> Data { get; set; } = new List<T>();
    
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    /// <remarks>
    /// Used to calculate total pages and determine if there are more pages.
    /// </remarks>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Calculated total number of pages
    /// </summary>
    /// <remarks>
    /// Calculated as: Ceiling(TotalCount / PageSize)
    /// Example: 100 items, 10 per page = 10 pages
    /// Example: 101 items, 10 per page = 11 pages
    /// </remarks>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    /// <summary>
    /// Indicates whether there is a previous page
    /// </summary>
    /// <remarks>
    /// True if Page > 1, false otherwise.
    /// Used by frontend to enable/disable "Previous" button.
    /// </remarks>
    public bool HasPreviousPage => Page > 1;
    
    /// <summary>
    /// Indicates whether there is a next page
    /// </summary>
    /// <remarks>
    /// True if Page < TotalPages, false otherwise.
    /// Used by frontend to enable/disable "Next" button.
    /// </remarks>
    public bool HasNextPage => Page < TotalPages;
}
