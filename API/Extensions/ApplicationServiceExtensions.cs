using API.Attributes;
using API.Data;
using API.Helpers;
using API.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DataContext>(opt =>
        {
            opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });

        services.AddCors();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
        services.AddSignalR();
        services.AddSingleton<PresenceTracker>();

        services.AddAutoRegister();

        return services;
    }

    private static void AddAutoRegister(this IServiceCollection services)
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsDefined(typeof(AutoRegisterAttribute), false));

        foreach (var type in types)
        {
            var attribute = type.GetCustomAttribute<AutoRegisterAttribute>()!;

            if (!attribute.AsInterface)
                services.AddScoped(type);
            else
            {
                var @interface = attribute.CustomInterface ?? type.GetInterfaces().Single();
                services.AddScoped(@interface, type);
            }
        }
    }
}
