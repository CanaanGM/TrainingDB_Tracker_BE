namespace DataLibrary.Helpers.Extensions;
public static class IQueryableExtensions
{
    /// <summary>
    /// Paginates an IQueryable query based on the specified page number and page size. 
    /// example: [queryable.Paginate(pageNumber, pageSize)]
    /// </summary>
    /// <param name="source">The source IQueryable.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The size of each page.</param>
    /// <typeparam name="T">The type of the queried objects.</typeparam>
    /// <returns>The paginated IQueryable.</returns>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> source, int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            pageNumber = 1;
        if (pageSize < 1)
            pageSize = 10; // default page size if not specified or negative

        return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

}
