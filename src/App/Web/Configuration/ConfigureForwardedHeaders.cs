using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace ManagedDoom.App.Web.Configuration;

internal sealed class ConfigureForwardedHeaders : IConfigureOptions<ForwardedHeadersOptions>
{
    public void Configure( ForwardedHeadersOptions options )
    {
        ArgumentNullException.ThrowIfNull( options );
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    }
}