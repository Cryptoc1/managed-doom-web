using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ManagedDoom.App.UI.Interop;

internal sealed class UserInputInterop( IJSRuntime runtime ) : Interop( runtime, "UserInput" )
{
    public ValueTask<UserInputReference> CreateUserInput( ElementReference element, CancellationToken cancellation = default ) => Access( ( module, cancellation ) =>
    {
#pragma warning disable IL2026
        var reference = module.Invoke<IJSInProcessObjectReference>(
            "createUserInput",
            element );
#pragma warning restore IL2026

        return ValueTask.FromResult<UserInputReference>( new( reference ) );
    }, cancellation );
}

internal sealed class UserInputReference( IJSInProcessObjectReference reference ) : IAsyncDisposable
{
    [DynamicDependency( DynamicallyAccessedMemberTypes.All, typeof( UserInputSnapshot ) )]
    public UserInputSnapshot GetSnapshot( )
    {
#pragma warning disable IL2026
        return reference.Invoke<UserInputSnapshot>( "capture" );
#pragma warning restore IL2026
    }

    public async ValueTask DisposeAsync( )
    {
        try
        {
            reference.InvokeVoid( "dispose" );
            await reference.DisposeAsync();
        }
        catch { }
    }

    public ValueTask RequestLock( CancellationToken cancellation = default ) => reference.InvokeVoidAsync( "requestLock", cancellation );
}

internal sealed record UserInputSnapshot(
    MouseDelta Delta,
    double Latency,
    IReadOnlyList<string> Pressed,
    IReadOnlyList<string> Released );

internal sealed record MouseDelta( float X, float Y )
{
    public static implicit operator MouseDelta( Vector2 value ) => new( value.X, value.Y );
    public static implicit operator Vector2( MouseDelta value ) => new( value.X, value.Y );
}