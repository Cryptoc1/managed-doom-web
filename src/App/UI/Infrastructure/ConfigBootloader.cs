using ManagedDoom.App.UI.Abstractions;

namespace ManagedDoom.App.UI.Infrastructure;

internal sealed class ConfigBootloader : IBootloader
{
    private const string LogSource = "CONFIG";

    public async Task OnBooting( BootloaderContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        context.Log( LogSource, "Initializing Default Config..." );

        var config = DefaultConfig();
        config.Save( "doom.cfg" );

        context.Log( LogSource, "Initialized Default Config!" );
    }

    private static Config DefaultConfig( ) => new()
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
}