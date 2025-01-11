namespace Core.Paginated;
public class PaginatedList<T> : List<T>
{
    public int Page { get; set; }

    public int PerPage { get; set; }

    public int TotalCount { get; set; }

    public PaginatedList(IEnumerable<T> enumerable) : base(enumerable)
    {
    }

    public PaginatedList(IEnumerable<T> enumerable, int page, int perPage, int totalCount) : base(enumerable)
    {
        Page = page;
        PerPage = perPage;
        TotalCount = totalCount;
    }
}