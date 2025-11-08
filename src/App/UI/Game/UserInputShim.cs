using System.Buffers;
using System.Numerics;
using ManagedDoom.UserInput;

namespace ManagedDoom.App.UI.Game;

internal sealed class UserInputShim( Config config ) : IDisposable, IUserInput
{
    private readonly bool[] weapons = ArrayPool<bool>.Shared.Rent( WeaponCount );
    private const int WeaponCount = 7;

    private MouseData mouse = new();
    private int isTurnHeld;

    public int MaxMouseSensitivity => 15;
    public int MouseSensitivity
    {
        get => config.mouse_sensitivity;
        set => config.mouse_sensitivity = value;
    }

    public HashSet<DoomMouseButton> PressedButtons { get; } = [];
    public HashSet<DoomKey> PressedKeys { get; } = [];

    public void BuildTicCmd( TicCmd cmd )
    {
        var keyForward = IsPressed( config.key_forward );
        var keyBackward = IsPressed( config.key_backward );
        var keyStrafeLeft = IsPressed( config.key_strafeleft );
        var keyStrafeRight = IsPressed( config.key_straferight );
        var keyTurnLeft = IsPressed( config.key_turnleft );
        var keyTurnRight = IsPressed( config.key_turnright );
        var keyFire = IsPressed( config.key_fire );
        var keyUse = IsPressed( config.key_use );
        var keyRun = IsPressed( config.key_run );
        var keyStrafe = IsPressed( config.key_strafe );

        weapons[ 0 ] = PressedKeys.Contains( DoomKey.Num1 );
        weapons[ 1 ] = PressedKeys.Contains( DoomKey.Num2 );
        weapons[ 2 ] = PressedKeys.Contains( DoomKey.Num3 );
        weapons[ 3 ] = PressedKeys.Contains( DoomKey.Num4 );
        weapons[ 4 ] = PressedKeys.Contains( DoomKey.Num5 );
        weapons[ 5 ] = PressedKeys.Contains( DoomKey.Num6 );
        weapons[ 6 ] = PressedKeys.Contains( DoomKey.Num7 );

        cmd.Clear();

        var strafe = keyStrafe;
        var speed = keyRun ? 1 : 0;
        var forward = 0;
        var side = 0;

        if( config.game_alwaysrun ) speed = 1 - speed;

        if( strafe )
        {
            if( keyTurnRight ) side += PlayerBehavior.SideMove[ speed ];
            if( keyTurnLeft ) side -= PlayerBehavior.SideMove[ speed ];
        }
        else
        {
            isTurnHeld = keyTurnLeft || keyTurnRight ? isTurnHeld + 1 : isTurnHeld = 0;
            var turnSpeed = isTurnHeld < PlayerBehavior.SlowTurnTics ? 2 : speed;

            if( keyTurnRight ) cmd.AngleTurn -= ( short )PlayerBehavior.AngleTurn[ turnSpeed ];
            if( keyTurnLeft ) cmd.AngleTurn += ( short )PlayerBehavior.AngleTurn[ turnSpeed ];
        }

        if( keyForward ) forward += PlayerBehavior.ForwardMove[ speed ];
        if( keyBackward ) forward -= PlayerBehavior.ForwardMove[ speed ];

        if( keyStrafeLeft ) side -= PlayerBehavior.SideMove[ speed ];
        if( keyStrafeRight ) side += PlayerBehavior.SideMove[ speed ];

        if( keyFire ) cmd.Buttons |= TicCmdButtons.Attack;
        if( keyUse ) cmd.Buttons |= TicCmdButtons.Use;

        // Check weapon keys.
        for( var i = 0; i < WeaponCount; i++ )
        {
            if( weapons[ i ] )
            {
                cmd.Buttons |= TicCmdButtons.Change;
                cmd.Buttons |= ( byte )(i << TicCmdButtons.WeaponShift);
                break;
            }
        }

        var ms = 0.5F * config.mouse_sensitivity;
        var mx = ( int )MathF.Round( ms * mouse.Delta.X );
        var my = ( int )MathF.Round( ms * -mouse.Delta.Y );
        forward += my;

        if( strafe ) side += mx * 2;
        else cmd.AngleTurn -= ( short )(mx * 0x16);

        if( forward > PlayerBehavior.MaxMove ) forward = PlayerBehavior.MaxMove;
        else if( forward < -PlayerBehavior.MaxMove ) forward = -PlayerBehavior.MaxMove;

        if( side > PlayerBehavior.MaxMove ) side = PlayerBehavior.MaxMove;
        else if( side < -PlayerBehavior.MaxMove ) side = -PlayerBehavior.MaxMove;

        cmd.ForwardMove += ( sbyte )forward;
        cmd.SideMove += ( sbyte )side;
    }

    public void Dispose( )
    {
        Reset();
        ArrayPool<bool>.Shared.Return( weapons );
    }

    public void GrabMouse( )
    {
    }

    private bool IsPressed( KeyBinding binding )
    {
        foreach( var key in binding.Keys )
        {
            if( PressedKeys.Contains( key ) ) return true;
        }

        foreach( var button in binding.MouseButtons )
        {
            if( PressedButtons.Contains( button ) ) return true;
        }

        return false;
    }

    public void MoveMouse( Vector2 delta )
    {
        mouse.PreviousPosition.X = mouse.Position.X;
        mouse.PreviousPosition.Y = mouse.Position.Y;
        mouse.Position += delta;

        mouse.Delta = mouse.Position - mouse.PreviousPosition;
        if( config.mouse_disableyaxis )
        {
            mouse.Delta.Y = 0;
        }
    }

    public void ReleaseMouse( )
    {
    }

    public void Reset( )
    {
        mouse = new();

        PressedButtons.Clear();
        PressedKeys.Clear();
    }

    public static readonly IReadOnlyDictionary<string, DoomMouseButton> ButtonMap = new Dictionary<string, DoomMouseButton>()
    {
        { "MouseLeft", DoomMouseButton.Mouse1 },
        { "MouseRight", DoomMouseButton.Mouse2 }
    };

    public static readonly IReadOnlyDictionary<string, DoomKey> KeyMap = new Dictionary<string, DoomKey>()
    {
        { "Space", DoomKey.Space },
        { "Comma", DoomKey.Comma },
        { "Minus", DoomKey.Subtract },
        { "Period", DoomKey.Period },
        { "Slash", DoomKey.Slash },
        { "Digit0", DoomKey.Num0 },
        { "Digit1", DoomKey.Num1 },
        { "Digit2", DoomKey.Num2 },
        { "Digit3", DoomKey.Num3 },
        { "Digit4", DoomKey.Num4 },
        { "Digit5", DoomKey.Num5 },
        { "Digit6", DoomKey.Num6 },
        { "Digit7", DoomKey.Num7 },
        { "Digit8", DoomKey.Num8 },
        { "Digit9", DoomKey.Num9 },
        { "Semicolon", DoomKey.Semicolon },
        { "NumpadEquals", DoomKey.Equal },
        { "KeyA", DoomKey.A },
        { "KeyB", DoomKey.B },
        { "KeyC", DoomKey.C },
        { "KeyD", DoomKey.D },
        { "KeyE", DoomKey.E },
        { "KeyF", DoomKey.F },
        { "KeyG", DoomKey.G },
        { "KeyH", DoomKey.H },
        { "KeyI", DoomKey.I },
        { "KeyJ", DoomKey.J },
        { "KeyK", DoomKey.K },
        { "KeyL", DoomKey.L },
        { "KeyM", DoomKey.M },
        { "KeyN", DoomKey.N },
        { "KeyO", DoomKey.O },
        { "KeyP", DoomKey.P },
        { "KeyQ", DoomKey.Q },
        { "KeyR", DoomKey.R },
        { "KeyS", DoomKey.S },
        { "KeyT", DoomKey.T },
        { "KeyU", DoomKey.U },
        { "KeyV", DoomKey.V },
        { "KeyW", DoomKey.W },
        { "KeyX", DoomKey.X },
        { "KeyY", DoomKey.Y },
        { "KeyZ", DoomKey.Z },
        { "Backslash", DoomKey.Backslash },
        { "BracketLeft", DoomKey.LBracket },
        { "BracketRight", DoomKey.RBracket },
        { "Escape", DoomKey.Escape },
        { "Enter", DoomKey.Enter },
        { "Tab", DoomKey.Tab },
        { "Backspace", DoomKey.Backspace },
        { "Insert", DoomKey.Insert },
        { "Delete", DoomKey.Delete },
        { "ArrowRight", DoomKey.Right },
        { "ArrowLeft", DoomKey.Left },
        { "ArrowDown", DoomKey.Down },
        { "ArrowUp", DoomKey.Up },
        { "PageUp", DoomKey.PageUp },
        { "PageDown", DoomKey.PageDown },
        { "Home", DoomKey.Home },
        { "End", DoomKey.End },
        { "Pause", DoomKey.Pause },
        { "F1", DoomKey.F1 },
        { "F2", DoomKey.F2 },
        { "F3", DoomKey.F3 },
        { "F4", DoomKey.F4 },
        { "F5", DoomKey.F5 },
        { "F6", DoomKey.F6 },
        { "F7", DoomKey.F7 },
        { "F8", DoomKey.F8 },
        { "F9", DoomKey.F9 },
        { "F10", DoomKey.F10 },
        { "F11", DoomKey.F11 },
        { "F12", DoomKey.F12 },
        { "Numpad0", DoomKey.Numpad0 },
        { "Numpad1", DoomKey.Numpad1 },
        { "Numpad2", DoomKey.Numpad2 },
        { "Numpad3", DoomKey.Numpad3 },
        { "Numpad4", DoomKey.Numpad4 },
        { "Numpad5", DoomKey.Numpad5 },
        { "Numpad6", DoomKey.Numpad6 },
        { "Numpad7", DoomKey.Numpad7 },
        { "Numpad8", DoomKey.Numpad8 },
        { "Numpad9", DoomKey.Numpad9 },
        { "NumpadDivide", DoomKey.Divide },
        { "NumpadMultiply", DoomKey.Multiply },
        { "NumpadMinus", DoomKey.Subtract },
        { "NumpadPlus", DoomKey.Add },
        { "NumpadEnter", DoomKey.Enter },
        { "ShiftLeft", DoomKey.LShift },
        { "ControlLeft", DoomKey.LControl },
        { "AltLeft", DoomKey.LAlt },
        { "ShiftRight", DoomKey.RShift },
        { "RightControl", DoomKey.RControl },
        { "AltRight", DoomKey.RAlt },
        { "ContextMenu", DoomKey.Menu}
    };

    private struct MouseData( )
    {
        public Vector2 Delta = new( 0, 0 );
        public Vector2 Position = new( 0, 0 );
        public Vector2 PreviousPosition = new( 0, 0 );
    }
}
