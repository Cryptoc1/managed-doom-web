namespace ManagedDoom.App.UI.Abstractions;

internal interface IBootloader
{
    public Task OnBooting( BootloaderContext context );
}

internal sealed class BootloaderContext( BootloaderLogger logger, IServiceProvider services )
{
    public IServiceProvider Services { get; } = services;

    public void Log( string source, string message ) => logger( source, message );
    public void Log( string message ) => logger( "OS", message );
}

internal delegate void BootloaderLogger( string source, string message );