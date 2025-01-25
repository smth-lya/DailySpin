namespace DailySpin.WebApi;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ContentTypeAttribute : Attribute
{
    public string ContentType { get; }

    public ContentTypeAttribute(string contentType)
    {
        ContentType = contentType;
    }
}

public static class HttpContentType
{
    public const string Js = "application/javascript";
    public const string Xml = "application/xml";
    public const string Json = "application/json";
    public const string Css = "text/css";
    public const string Html = "text/html";
    public const string PlainText = "text/plain";
    public const string Png = "image/png";
    public const string Jpg = "image/jpeg";
    public const string Jpeg = "image/jpeg";
    public const string Gif = "image/gif";
    public const string Svg = "image/svg+xml";
    public const string Ico = "image/x-icon";
    public const string OctetStream = "application/octet-stream";
}