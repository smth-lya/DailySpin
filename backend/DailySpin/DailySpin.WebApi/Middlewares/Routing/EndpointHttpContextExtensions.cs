using DailySpin.WebApi;
using System.Collections;
using System.Diagnostics;
/// <summary>
/// Extension methods to expose Endpoint on HttpContext.
/// </summary>
public static class EndpointHttpContextExtensions
{
    /// <summary>
    /// Extension method for getting the <see cref="Endpoint"/> for the current request.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> context.</param>
    /// <returns>The <see cref="Endpoint"/> or <c>null</c> if the request doesn't have an endpoint.</returns>
    /// <remarks>
    /// The endpoint for a request is typically set by routing middleware. A request might not have
    /// an endpoint if routing middleware hasn't run yet, or the request didn't match a route.
    /// </remarks>
    public static Endpoint? GetEndpoint(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Features.Get<IEndpointFeature>()?.Endpoint;
    }

    /// <summary>
    /// Extension method for setting the <see cref="Endpoint"/> for the current request.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> context.</param>
    /// <param name="endpoint">The <see cref="Endpoint"/>. A <c>null</c> value clears the endpoint for the current request.</param>
    public static void SetEndpoint(this HttpContext context, Endpoint? endpoint)
    {
        ArgumentNullException.ThrowIfNull(context);

        var feature = context.Features.Get<IEndpointFeature>();

        if (endpoint != null)
        {
            if (feature == null)
            {
                feature = new EndpointFeature();
                context.Features.Set(feature);
            }

            feature.Endpoint = endpoint;
        }
        else
        {
            if (feature == null)
            {
                // No endpoint to set and no feature on context. Do nothing
                return;
            }

            feature.Endpoint = null;
        }
    }

    private sealed class EndpointFeature : IEndpointFeature
    {
        public Endpoint? Endpoint { get; set; }
    }
}
/// <summary>
/// Default implementation for <see cref="IFeatureCollection"/>.
/// </summary>
public class FeatureCollection : IFeatureCollection
{
    private static readonly KeyComparer FeatureKeyComparer = new KeyComparer();
    private readonly IFeatureCollection? _defaults;
    private readonly int _initialCapacity;
    private IDictionary<Type, object>? _features;
    private volatile int _containerRevision;

    /// <summary>
    /// Initializes a new instance of <see cref="FeatureCollection"/>.
    /// </summary>
    public FeatureCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FeatureCollection"/> with the specified initial capacity.
    /// </summary>
    /// <param name="initialCapacity">The initial number of elements that the collection can contain.</param>
    /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="initialCapacity"/> is less than 0</exception>
    public FeatureCollection(int initialCapacity)
    {
        //ArgumentOutOfRangeThrowHelper.ThrowIfNegative(initialCapacity);

        _initialCapacity = initialCapacity;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FeatureCollection"/> with the specified defaults.
    /// </summary>
    /// <param name="defaults">The feature defaults.</param>
    public FeatureCollection(IFeatureCollection defaults)
    {
        _defaults = defaults;
    }

    /// <inheritdoc />
    public virtual int Revision
    {
        get { return _containerRevision + (_defaults?.Revision ?? 0); }
    }

    /// <inheritdoc />
    public bool IsReadOnly { get { return false; } }

    /// <inheritdoc />  
    public object? this[Type key]
    {
        get
        {
            //ArgumentNullThrowHelper.ThrowIfNull(key);

            return _features != null && _features.TryGetValue(key, out var result) ? result : _defaults?[key];
        }
        set
        {
            //ArgumentNullThrowHelper.ThrowIfNull(key);

            if (value == null)
            {
                if (_features != null && _features.Remove(key))
                {
                    _containerRevision++;
                }
                return;
            }

            if (_features == null)
            {
                _features = new Dictionary<Type, object>(_initialCapacity);
            }
            _features[key] = value;
            _containerRevision++;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
    {
        if (_features != null)
        {
            foreach (var pair in _features)
            {
                yield return pair;
            }
        }

        if (_defaults != null)
        {
            // Don't return features masked by the wrapper.
            foreach (var pair in _features == null ? _defaults : _defaults.Except(_features, FeatureKeyComparer))
            {
                yield return pair;
            }
        }
    }

    /// <inheritdoc />
    public TFeature? Get<TFeature>()
    {
        if (typeof(TFeature).IsValueType)
        {
            var feature = this[typeof(TFeature)];
            if (feature is null && Nullable.GetUnderlyingType(typeof(TFeature)) is null)
            {
                throw new InvalidOperationException(
                    $"{typeof(TFeature).FullName} does not exist in the feature collection " +
                    $"and because it is a struct the method can't return null. Use 'featureCollection[typeof({typeof(TFeature).FullName})] is not null' to check if the feature exists.");
            }
            return (TFeature?)feature;
        }
        return (TFeature?)this[typeof(TFeature)];
    }

    /// <inheritdoc />
    public void Set<TFeature>(TFeature? instance)
    {
        this[typeof(TFeature)] = instance;
    }

    // Used by the debugger. Count over enumerable is required to get the correct value.
    private int GetCount() => this.Count();

    private sealed class KeyComparer : IEqualityComparer<KeyValuePair<Type, object>>
    {
        public bool Equals(KeyValuePair<Type, object> x, KeyValuePair<Type, object> y)
        {
            return x.Key.Equals(y.Key);
        }

        public int GetHashCode(KeyValuePair<Type, object> obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}