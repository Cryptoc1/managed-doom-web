using System.Runtime.CompilerServices;
using ManagedDoom.Video;
using SkiaSharp;

namespace ManagedDoom.App.UI.Game;

internal sealed class VideoShim( Config config, GameContent content ) : IDisposable, IVideo
{
    private readonly SKPaint paint = new()
    {
        IsAntialias = false,
        IsDither = false,
        IsStroke = false,
    };

    private readonly Renderer renderer = new( config, content );
    private readonly SKBitmap scene = CreateScene( config.video_highresolution ? (400, 640) : (200, 320) );
    private readonly TransformMatrix transform;

    public bool DisplayMessage
    {
        get => renderer.DisplayMessage;
        set => renderer.DisplayMessage = value;
    }

    public int MaxGammaCorrectionLevel => renderer.MaxGammaCorrectionLevel;
    public int GammaCorrectionLevel
    {
        get => renderer.GammaCorrectionLevel;
        set => renderer.GammaCorrectionLevel = value;
    }

    public int MaxWindowSize => renderer.MaxWindowSize;
    public int WindowSize
    {
        get => renderer.WindowSize;
        set => renderer.WindowSize = value;
    }

    public int WipeBandCount => renderer.WipeBandCount;
    public int WipeHeight => renderer.WipeHeight;

    private static SKBitmap CreateScene( (int Width, int Height) size )
    {
        var info = new SKImageInfo(
            size.Width,
            size.Height,
            SKColorType.Rgba8888,
            SKAlphaType.Unpremul );

        return new( info, SKBitmapAllocFlags.ZeroPixels );
    }

    public void Dispose( )
    {
        paint.Dispose();
        scene.Dispose();
    }

    public bool HasFocus( ) => true;
    public void InitializeWipe( ) => renderer.InitializeWipe();

    public void Render( Doom doom, Fixed frame )
    {
        renderer.Render(
            doom,
            scene.GetPixelSpan(),
            frame );

        scene.NotifyPixelsChanged();
    }

    [MethodImpl( MethodImplOptions.AggressiveOptimization )]
    public void Render( SKCanvas canvas, Doom doom, Fixed frame )
    {
        Render( doom, frame );

        canvas.Save();

        transform.Apply( canvas, out var bounds );
        canvas.DrawBitmap(
            scene,
            new SKRect( 0, 0, bounds.Height, bounds.Width ),
            paint );

        canvas.Restore();
    }

    private struct TransformMatrix( )
    {
        private float height;
        private SKMatrix? value;

        public void Apply( SKCanvas canvas, out SKRect bounds )
        {
            ArgumentNullException.ThrowIfNull( canvas );

            bounds = canvas.LocalClipBounds;
            if( bounds.Height != height || !value.HasValue )
            {
                height = bounds.Height;
                value = Create( bounds.Height );
            }

            canvas.SetMatrix( canvas.TotalMatrix.PostConcat( value.Value ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization )]
        private static SKMatrix Create( float height )
        {
            var matrix = SKMatrix.CreateIdentity();

            SKMatrix.Concat( ref matrix, matrix, SKMatrix.CreateRotationDegrees( 90 ) );
            SKMatrix.Concat( ref matrix, matrix, SKMatrix.CreateTranslation( 0, -height ) );
            SKMatrix.Concat( ref matrix, matrix, SKMatrix.CreateScale( 1, -1 ) );
            SKMatrix.Concat( ref matrix, matrix, SKMatrix.CreateTranslation( 0, -height ) );

            return matrix;
        }
    }
}