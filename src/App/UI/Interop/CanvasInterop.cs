using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ManagedDoom.App.UI.Interop;

internal sealed class CanvasInterop( IJSRuntime runtime ) : Interop( runtime, "Canvas" )
{
    public ValueTask<CanvasContextReference> CreateContext( ElementReference canvas, CancellationToken cancellation = default ) => Access<CanvasContextReference>( async ( module, cancellation ) =>
    {
        var reference = await module.InvokeAsync<IJSInProcessObjectReference>(
            "createContext",
            cancellation,
            canvas );

        return new( reference );
    }, cancellation );
}

internal sealed class CanvasContextReference( IJSInProcessObjectReference reference ) : IAsyncDisposable
{
    public async ValueTask DisposeAsync( )
    {
        try
        {
            await reference.DisposeAsync();
        }
        catch( JSDisconnectedException ) { }
    }

#pragma warning disable IL2026
    [DynamicDependency( DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicProperties, typeof( DOMRect ) )]
    public DOMRect GetBoundingClientRect( ) => reference.Invoke<DOMRect>( "getBoundingClientRect" );
#pragma warning restore IL2026

    public void PutImageData( byte[] data ) => reference.InvokeVoid( "putImageData", data );
}