
using Material.Blazor;
using Microsoft.Extensions.DependencyInjection;

namespace CovidStatsCH.Components
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddCovidStatsCHServices(this IServiceCollection services)
        {
            services.AddDevExpressBlazor();
            return services
                .AddMBServices()
                .AddSingleton<DataPointProvider>();
        }
    }
}
