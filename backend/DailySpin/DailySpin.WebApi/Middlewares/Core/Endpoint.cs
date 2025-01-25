namespace DailySpin.WebApi;

public class Endpoint
{
    public Endpoint(RequestDelegate? requestDelegate, EndpointMetadataCollection? metadata, string? displayName)
    {
        RequestDelegate = requestDelegate;
        Metadata = metadata ?? EndpointMetadataCollection.Empty;
        DisplayName = displayName;
    }

    public string? DisplayName { get; }

    /// <summary>
    /// Gets the collection of metadata associated with this endpoint.
    /// </summary>
    public EndpointMetadataCollection Metadata { get; }

    /// <summary>
    /// Gets the delegate used to process requests for the endpoint.
    /// </summary>
    public RequestDelegate? RequestDelegate { get; }

    public override string? ToString() => DisplayName ?? base.ToString();
}
