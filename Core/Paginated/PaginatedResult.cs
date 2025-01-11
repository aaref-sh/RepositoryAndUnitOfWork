namespace Core.Paginated;

public class PaginatedResult<T>(List<T> list, int currentPage, int itemsPerPage, int totalItems)
{
    public List<T> Data { get; set; } = list;
    public int CurrentPage { get; set; } = currentPage;
    public int ItemsPerPage { get; set; } = itemsPerPage;
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)ItemsPerPage);
    public int TotalItems { get; set; } = totalItems;
}
