using System.Collections.Frozen;
using System.Collections.Immutable;

namespace LoupixDeck.Services.Commands;

/// <summary>
/// Produces placeholder default values for command parameters that the user
/// has not supplied yet (used by the command builder when materializing a
/// command string from a menu selection).
/// </summary>
public static class ParameterDefaults
{
    private static readonly FrozenDictionary<Type, object?> Lookup = FrozenDictionary.ToFrozenDictionary<Type, object?>([
            new(typeof(string), "string"),
            new(typeof(bool), false),
            new(typeof(char), '\0'),
            new(typeof(byte), (byte)0),
            new(typeof(sbyte), (sbyte)0),
            new(typeof(short), (short)0),
            new(typeof(ushort), (ushort)0),
            new(typeof(int), 0),
            new(typeof(uint), 0U),
            new(typeof(long), 0L),
            new(typeof(ulong), 0UL),
            new(typeof(float), 0f),
            new(typeof(double), 0.0),
            new(typeof(decimal), 0m),
            new(typeof(DateTime), default(DateTime)),
            new(typeof(Guid), Guid.Empty),
        ]);

    private static ImmutableDictionary<Type, object?> EnumLookup = ImmutableDictionary<Type, object?>.Empty;

    public static object? GetDefaultValue(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        if (Lookup.TryGetValue(type, out var value))
            return value;

        if (type.IsEnum)
        {
            return ImmutableInterlocked.GetOrAdd(ref EnumLookup, type, static type =>
            {
                var values = Enum.GetValues(type);
                return values.Length > 0
                    ? values.GetValue(0)!
                    : Activator.CreateInstance(type)!;
            });
        }

        if (Nullable.GetUnderlyingType(type) != null)
            return null;

        if (type.IsValueType)
            return Activator.CreateInstance(type)!;

        return null;
    }
}
