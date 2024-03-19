using API.Helpers;
using System.Text.Json;

namespace API.Extensions;

public static class HttpExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
    {
        response.Headers.Append("Pagination", JsonSerializer.Serialize(header, JsonOptions));
        response.Headers.Append("Access-Control-Expose-Headers", "Pagination");
    }
}
