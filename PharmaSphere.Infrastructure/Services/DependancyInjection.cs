using Microsoft.Extensions.DependencyInjection;
using PharmaSphere.Application.Interfaces.Services;
using PharmaSphere.Infrastructure.Services;

namespace PharmaSphere.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<InventoryLockService>();

            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IPosService, PosService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ISmartDispensingService, SmartDispensingService>();

            return services;
        }
    }
}