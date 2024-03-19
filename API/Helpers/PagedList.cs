using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

public class PagedList<T>(int count, int pageNumber, int pageSize) : List<T>
{
    public int CurrentPage { get; } = pageNumber;
    public int TotalPages { get; } = (int)Math.Ceiling(count / (double)pageSize);
    public int PageSize { get; } = pageSize;
    public int TotalCount { get; } = count;

    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedList<T>(count, pageNumber, pageSize);
        result.AddRange(items);

        return result;
    }
}
