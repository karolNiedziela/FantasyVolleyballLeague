using System.Reflection;

namespace FantasyVolleyballLeague.DataApi.Common.Handlers
{
    internal static class HandlerExtensions
    {
        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            var handlerTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract);

            foreach (var type in handlerTypes)
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));

                foreach (var serviceInterface in interfaces)
                {
                    services.AddScoped(type);
                    services.AddScoped(serviceInterface, sp => sp.GetRequiredService(type));
                }
            }

            return services;
        }
    }
}
