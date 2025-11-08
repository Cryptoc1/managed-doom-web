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

    public DoomShim( )
    {
        config = new()
        {
            audio_randompitch = true,
            audio_soundvolume = 8,

            game_alwaysrun = true,

            key_backward = KeyBinding.Parse( "s,down" ),
            key_fire = KeyBinding.Parse( "mouse1,f,lcontrol,rcontrol" ),
            key_forward = KeyBinding.Parse( "w,up" ),
            key_run = KeyBinding.Parse( "lshift,rshift" ),
            key_strafe = KeyBinding.Parse( "lalt,ralt" ),
            key_strafeleft = KeyBinding.Parse( "a" ),
            key_straferight = KeyBinding.Parse( "d" ),
            key_turnleft = KeyBinding.Parse( "left" ),
            key_turnright = KeyBinding.Parse( "right" ),
            key_use = KeyBinding.Parse( "space,mouse2" ),

            mouse_disableyaxis = false,
            mouse_sensitivity = 7,

            video_displaymessage = true,
            video_fpsscale = 1,
            video_fullscreen = true,
            video_gamescreensize = 7,
            video_gammacorrection = 2,
            video_highresolution = false,
        };

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
