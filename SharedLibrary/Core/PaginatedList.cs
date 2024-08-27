namespace SharedLibrary.Core;
public class PaginatedList<T>
{
    public List<T> Items { get; set; }
    public PaginationMetadata Metadata { get; set; }
}

public class PaginationMetadata
{
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
