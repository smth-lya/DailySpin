using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DailySpin.WebApi;

public sealed class EndpointMetadataCollection : IReadOnlyList<object>
{
    public static readonly EndpointMetadataCollection Empty = new EndpointMetadataCollection(Array.Empty<object>());

    private readonly object[] _items;
    private readonly ConcurrentDictionary<Type, object[]> _cache;

    public EndpointMetadataCollection(IEnumerable<object> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _items = items.ToArray();
        _cache = new ConcurrentDictionary<Type, object[]>();
    }

    public EndpointMetadataCollection(params object[] items)
        : this((IEnumerable<object>)items)
    {
    }

    public object this[int index] => _items[index];

    public int Count => _items.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? GetMetadata<T>() where T : class
    {
        if (_cache.TryGetValue(typeof(T), out var obj))
        {
            var result = (T[])obj;
            var length = result.Length;
            return length > 0 ? result[length - 1] : default;
        }

        return GetMetadataSlow<T>();
    }

    private T? GetMetadataSlow<T>() where T : class
    {
        var result = GetOrderedMetadataSlow<T>();
        var length = result.Length;
        return length > 0 ? result[length - 1] : default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IReadOnlyList<T> GetOrderedMetadata<T>() where T : class
    {
        if (_cache.TryGetValue(typeof(T), out var result))
        {
            return (T[])result;
        }

        return GetOrderedMetadataSlow<T>();
    }

    private T[] GetOrderedMetadataSlow<T>() where T : class
    {
        // Perf: avoid allocations totally for the common case where there are no matching metadata.
        List<T>? matches = null;

        var items = _items;
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i] is T item)
            {
                matches ??= new List<T>();
                matches.Add(item);
            }
        }

        var results = matches == null ? Array.Empty<T>() : matches.ToArray();
        _cache.TryAdd(typeof(T), results);
        return results;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetRequiredMetadata<T>() where T : class
    {
        var metadata = GetMetadata<T>();
        return metadata ?? throw new InvalidOperationException($"Metadata '{typeof(T)}' is not found.");
    }

    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator<object> IEnumerable<object>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<object>
    {
        // Intentionally not readonly to prevent defensive struct copies
        private object[] _items;
        private int _index;
        private object? _current;

        internal Enumerator(EndpointMetadataCollection collection)
        {
            _items = collection._items;
            _index = 0;
            _current = null;
        }

        public object Current => _current!;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_index < _items.Length)
            {
                _current = _items[_index++];
                return true;
            }

            _current = null;
            return false;
        }

        public void Reset()
        {
            _index = 0;
            _current = null;
        }
    }
}
