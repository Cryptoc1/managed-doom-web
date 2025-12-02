using System.Runtime.ExceptionServices;
using ManagedDoom.App.UI.Interop;
using SkiaSharp.Views.Blazor;

namespace ManagedDoom.App.UI.Game;

internal sealed class DoomShim : IDisposable
{
    private readonly Config config;
    private GameContent content;
    private Doom doom;
    private FrameState frame;
    private UserInputShim userInput;
    private VideoShim video;

    public DoomShim( Config config )
    {
        this.config = config;

        try
        {
            var args = new CommandLineArgs( [ "iwad", "DOOM1.WAD" ] );
            content = new( args );
            userInput = new( config );
            video = new( config, content );

            doom = new( args, config, content, video, default, default, userInput );
            frame = new( args.timedemo.Present ? 1 : config.video_fpsscale );
        }
        catch( Exception e )
        {
            Dispose();
            ExceptionDispatchInfo.Throw( e );
        }
    }

    public void Dispose( )
    {
        if( doom is not null )
        {
            doom = default!;
        }

        if( video is not null )
        {
            video.Dispose();
            video = default!;
        }

        if( userInput is not null )
        {
            userInput.Dispose();
            userInput = default!;
        }

        if( content is not null )
        {
            content.Dispose();
            content = default!;
        }

        frame = default!;
    }

    public bool Update( SKPaintGLSurfaceEventArgs e, UserInputReference input )
    {
        ArgumentNullException.ThrowIfNull( e );

        UpdateConfig( config, e );
        UpdateInput( doom, userInput, input );

        if( frame.Update() )
        {
            if( doom.Update() is UpdateResult.Completed )
            {
                return false;
            }

            video.Render(
                e.Surface.Canvas,
                doom!,
                frame.Value() );
        }

        return true;
    }

    private static void UpdateConfig( Config config, SKPaintGLSurfaceEventArgs e )
    {
        config.video_screenheight = e.Info.Height;
        config.video_screenwidth = e.Info.Width;
    }

    private static void UpdateInput( Doom doom, UserInputShim shim, UserInputReference input )
    {
        var snapshot = input.GetSnapshot();

        shim.MoveMouse( snapshot.Delta );
        foreach( var (code, key) in UserInputShim.KeyMap )
        {
            if( snapshot.Pressed.Contains( code ) ) PressKey( key );
            if( snapshot.Released.Contains( code ) ) ReleaseKey( key );
        }

        foreach( var (code, button) in UserInputShim.ButtonMap )
        {
            if( snapshot.Pressed.Contains( code ) ) PressButton( button );
            if( snapshot.Released.Contains( code ) ) ReleaseButton( button );
        }

        void PressButton( DoomMouseButton button )
        {
            if( button is not DoomMouseButton.Unknown )
            {
                shim.PressedButtons.Add( button );
                doom.PostEvent( new( EventType.Mouse, DoomKey.Unknown ) );
            }
        }

        void PressKey( DoomKey key )
        {
            if( key is not DoomKey.Unknown )
            {
                shim.PressedKeys.Add( key );
                doom.PostEvent( new( EventType.KeyDown, key ) );
            }
        }

        void ReleaseButton( DoomMouseButton button )
        {
            if( button is not DoomMouseButton.Unknown )
            {
                shim.PressedButtons.Remove( button );
                doom.PostEvent( new( EventType.Mouse, DoomKey.Unknown ) );
            }
        }

        void ReleaseKey( DoomKey key )
        {
            if( key is not DoomKey.Unknown )
            {
                shim.PressedKeys.Remove( key );
                doom.PostEvent( new( EventType.KeyUp, key ) );
            }
        }
    }

    private struct FrameState( int scale = 1 )
    {
        private int count = -1;

        public bool Update( ) => ((count += 1) % scale) is 0;
        public readonly Fixed Value( ) => Fixed.FromInt( (count % scale) + 1 ) / scale;
    }
}
