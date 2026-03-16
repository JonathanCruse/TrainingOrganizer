using System.Reflection;
using System.Runtime.CompilerServices;

namespace TrainingOrganizer.Infrastructure.Persistence.Documents;

/// <summary>
/// Uses reflection to reconstruct domain objects that have private constructors
/// marked with [SetsRequiredMembers]. This is the pragmatic approach for the
/// Infrastructure layer to rehydrate aggregates from persistence.
/// </summary>
internal static class DomainObjectMapper
{
    /// <summary>
    /// Creates an uninitialized instance of T and invokes its private parameterless constructor.
    /// Works with classes that have [SetsRequiredMembers] private constructors.
    /// </summary>
    internal static T CreateInstance<T>() where T : class
    {
        var constructor = typeof(T).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        if (constructor is null)
            throw new InvalidOperationException(
                $"Type {typeof(T).Name} does not have a private parameterless constructor.");

        return (T)constructor.Invoke(null);
    }

    /// <summary>
    /// Sets a property value on an object, including private setters and backing fields.
    /// </summary>
    internal static void SetProperty<T>(T obj, string propertyName, object? value) where T : class
    {
        var property = typeof(T).GetProperty(propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property is not null && property.CanWrite)
        {
            property.SetValue(obj, value);
            return;
        }

        // Try init-only via backing field
        if (property is not null)
        {
            // For auto-properties with init, try the compiler-generated backing field
            var backingField = typeof(T).GetField(
                $"<{propertyName}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (backingField is not null)
            {
                backingField.SetValue(obj, value);
                return;
            }
        }

        // Try direct field access (for manually declared backing fields like _name)
        var field = typeof(T).GetField(
            $"_{char.ToLowerInvariant(propertyName[0])}{propertyName[1..]}",
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (field is not null)
        {
            field.SetValue(obj, value);
            return;
        }

        throw new InvalidOperationException(
            $"Cannot set property '{propertyName}' on type {typeof(T).Name}.");
    }

    /// <summary>
    /// Sets a field value directly on an object.
    /// </summary>
    internal static void SetField<T>(T obj, string fieldName, object? value) where T : class
    {
        var type = typeof(T);

        // Walk up the inheritance hierarchy
        while (type is not null)
        {
            var field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field is not null)
            {
                field.SetValue(obj, value);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException(
            $"Cannot find field '{fieldName}' on type {typeof(T).Name} or its base types.");
    }

    /// <summary>
    /// Adds items to a private list field on an object.
    /// </summary>
    internal static void AddToList<TObj, TItem>(TObj obj, string fieldName, IEnumerable<TItem> items) where TObj : class
    {
        var type = typeof(TObj);

        while (type is not null)
        {
            var field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field is not null)
            {
                var list = field.GetValue(obj) as List<TItem>;
                list?.AddRange(items);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException(
            $"Cannot find list field '{fieldName}' on type {typeof(TObj).Name}.");
    }

    /// <summary>
    /// Adds items to a private HashSet field on an object.
    /// </summary>
    internal static void AddToHashSet<TObj, TItem>(TObj obj, string fieldName, IEnumerable<TItem> items) where TObj : class
    {
        var type = typeof(TObj);

        while (type is not null)
        {
            var field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field is not null)
            {
                var set = field.GetValue(obj) as HashSet<TItem>;
                if (set is not null)
                {
                    foreach (var item in items)
                        set.Add(item);
                }
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException(
            $"Cannot find HashSet field '{fieldName}' on type {typeof(TObj).Name}.");
    }
}
