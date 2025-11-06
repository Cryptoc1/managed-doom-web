using System.Buffers;
using ManagedDoom.UserInput;

namespace ManagedDoom.App.UI.Game;

internal sealed class UserInputShim( Config config ) : IDisposable, IUserInput
{
    private readonly bool[] weapons = ArrayPool<bool>.Shared.Rent( 7 );

    public int MaxMouseSensitivity => 15;
    public int MouseSensitivity
    {
        get => config.mouse_sensitivity;
        set => config.mouse_sensitivity = value;
    }

    public void BuildTicCmd( TicCmd cmd ) => throw new NotImplementedException();

    public void Dispose( )
    {
        ArrayPool<bool>.Shared.Return( weapons );
    }

    public void GrabMouse( ) => throw new NotImplementedException();
    public void ReleaseMouse( ) => throw new NotImplementedException();
    public void Reset( ) => throw new NotImplementedException();
}