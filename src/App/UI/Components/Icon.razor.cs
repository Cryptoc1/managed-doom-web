namespace ManagedDoom.App.UI.Components;

public readonly struct IconName( string name ) : IEquatable<IconName>
{
    private readonly string name = name;

    public static readonly IconName Settings = new( "settings" );

    /// <inheritdoc/>
    public bool Equals( IconName other ) => name.Equals( other.name, StringComparison.Ordinal );

    /// <inheritdoc/>
    public override bool Equals( object? obj ) => obj is IconName icon && Equals( icon );

    /// <inheritdoc/>
    public override string ToString( ) => name;

    public static bool operator ==( IconName left, IconName right ) => left.Equals( right );
    public static bool operator !=( IconName left, IconName right ) => !(left == right);

    public override int GetHashCode( ) => name.GetHashCode();
}