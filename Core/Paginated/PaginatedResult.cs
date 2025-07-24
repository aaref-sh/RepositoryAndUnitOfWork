namespace Core.Paginated;

public class PaginatedResult<T>
{
    public List<T> Data { get; set; } = [];
    public int CurrentPage { get; set; }
    public int ItemsPerPage { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)ItemsPerPage);

    // Empty constructor
    public PaginatedResult()
    {
    }

    // Constructor with parameters
    public PaginatedResult(List<T> data, int currentPage, int itemsPerPage, int totalItems)
    {
        Data = data;
        CurrentPage = currentPage;
        ItemsPerPage = itemsPerPage;
        TotalItems = totalItems;
    }
}
