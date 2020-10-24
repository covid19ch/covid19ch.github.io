
using Material.Blazor;
using Microsoft.Extensions.DependencyInjection;

namespace CovidStatsCH.Components
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddCovidStatsCHGameServices(this IServiceCollection services)
        {
            return services
                .AddMBServices()
                .AddSingleton<DataPointProvider>();
        }
    }
}
