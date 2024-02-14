using System.Text.Json;
using API.Helpers;

namespace API.Extensions;

public static class HttpExtensions
{
  private static readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

  public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
  {
    response.Headers.Append("Pagination", JsonSerializer.Serialize(header, _options));
    response.Headers.Append("Access-Control-Expose-Headers", "Pagination");
  }
}
