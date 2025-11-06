using ManagedDoom.App.UI.Abstractions;

namespace ManagedDoom.App.UI.Infrastructure;

internal sealed class WadBootloader( HttpClient http ) : IBootloader
{
    private const string LogSource = "WAD";

    public async Task OnBooting( BootloaderContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        context.Log( LogSource, "Loading WAD..." );

        await using( var data = await http.GetStreamAsync( "DOOM1.WAD" ) )
        await using( var file = File.OpenWrite( "DOOM1.WAD" ) )
        {
            await data.CopyToAsync( file );
        }

        context.Log( LogSource, "Loaded WAD!" );
    }
}