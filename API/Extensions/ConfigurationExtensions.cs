using System.Text;

namespace API.Extensions;

public static class ConfigurationExtensions
{
    public static byte[] GetTokenKey(this IConfiguration config) => Encoding.UTF8.GetBytes(config["TokenKey"]!);
}
