using System.Net;
namespace DailySpin.Framework;

public interface IHandler
{
    //void Handle(Stream stream, Request request);
    Task HandleAsync(HttpListenerRequest request, HttpListenerResponse response);
}