using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ManagedDoom.App.UI.Interop;

internal sealed class UserInputInterop( IJSRuntime runtime ) : Interop( runtime, "UserInput" )
{
    public ValueTask<UserInputReference> CreateUserInput( ElementReference element, CancellationToken cancellation = default ) => Access<UserInputReference>( async ( module, cancellation ) =>
    {
        var snapshot = new UserInputSnapshotReference();
        try
        {
            var reference = await module.InvokeAsync<IJSInProcessObjectReference>(
                "createUserInput",
                cancellation,
                element,
                snapshot.Reference );

            return new( reference, snapshot );
        }
        catch
        {
            snapshot.Dispose();
            throw;
        }
    }, cancellation );
}

internal sealed class UserInputSnapshotReference : IDisposable
{
    public DotNetObjectReference<UserInputSnapshotReference> Reference { get; }

    public UserInputSnapshot Value { get; private set; } = new( Vector2.Zero, 0, [], [] );

    [DynamicDependency( nameof( OnSnapshot ) )]
    public UserInputSnapshotReference( )
    {
        Reference = DotNetObjectReference.Create( this );
    }

    public void Dispose( ) => Reference.Dispose();

    [JSInvokable]
    public void OnSnapshot( UserInputSnapshot e )
    {
        Console.WriteLine( e.Delta );
        Value = e;
    }
}

internal sealed class UserInputReference( IJSInProcessObjectReference reference, UserInputSnapshotReference snapshot ) : IAsyncDisposable
{
    public UserInputSnapshot GetSnapshot( ) => snapshot.Value;

    public async ValueTask DisposeAsync( )
    {
        try
        {
            reference.InvokeVoid( "dispose" );
            await reference.DisposeAsync();
        }
        catch { }

        snapshot.Dispose();
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