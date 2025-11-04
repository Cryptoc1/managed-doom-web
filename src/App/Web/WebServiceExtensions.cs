using ManagedDoom.App.UI;
using ManagedDoom.App.Web.Configuration;

namespace ManagedDoom.App.Web;

internal static class WebServiceExtensions
{
    public static TBuilder WithManagedDoomWeb<TBuilder>( this TBuilder builder )
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull( builder );

        builder.Services.AddCors()
            .AddRequestDecompression()
            .AddRequestTimeouts()
            .AddResponseCaching()
            .AddResponseCompression()
            .AddRouting()
            .AddControllersWithViews();

        builder.Services.AddManagedDoomUI()
            .AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.ConfigureOptions<ConfigureCookiePolicy>()
            .ConfigureOptions<ConfigureForwardedHeaders>()
            .ConfigureOptions<ConfigureJson>()
            .ConfigureOptions<ConfigureRequestTimeouts>()
            .ConfigureOptions<ConfigureResponseCompression>()
            .ConfigureOptions<ConfigureRouting>();

        return builder;
    }
}