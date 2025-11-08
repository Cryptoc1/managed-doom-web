using System.Diagnostics.CodeAnalysis;
using ManagedDoom.App.UI.Interop;

namespace ManagedDoom.App.UI;

/// <summary> Extensions for registering services required by Wadio Components. </summary>
public static class UIServiceExtensions
{
    /// <summary> Register service required by Wadio Components. </summary>
    [DynamicDependency( DynamicallyAccessedMemberTypes.All, typeof( AppRoot ) )]
    public static IServiceCollection AddManagedDoomUI( this IServiceCollection services )
    {
        ArgumentNullException.ThrowIfNull( services );

        return services.AddScoped<UserInputInterop>();
    }
}