namespace API.Extensions;

public static class EnumerableExtensions
{
    public static bool None<T>(this IEnumerable<T> source) => !source.Any();
    public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate) => !source.Any(predicate);
}
