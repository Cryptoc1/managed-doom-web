using System.Numerics;
using System.Text.Json.Serialization;
using ManagedDoom.App.UI.Abstractions;

namespace ManagedDoom.App.UI.Json;

[JsonSerializable( typeof( UIVersion ) )]
[JsonSerializable( typeof( Vector2 ) )]

[JsonSourceGenerationOptions( DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = false )]
public sealed partial class UIJsonContext : JsonSerializerContext;