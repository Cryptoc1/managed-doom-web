using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace ManagedDoom.App.UI;

/// <summary> Extensions for registering services required by Wadio Components. </summary>
public static class UIServiceExtensions
{
    /// <summary> Register service required by Wadio Components. </summary>
    [DynamicDependency( DynamicallyAccessedMemberTypes.All, typeof( AppRoot ) )]
    [DynamicDependency( DynamicallyAccessedMemberTypes.All, typeof( ImmutableArray<> ) )]
    [DynamicDependency( DynamicallyAccessedMemberTypes.All, typeof( ImmutableDictionary<,> ) )]
    public static IServiceCollection AddManagedDoomUI( this IServiceCollection services )
    {
        ArgumentNullException.ThrowIfNull( services );

        return services;
    }
}