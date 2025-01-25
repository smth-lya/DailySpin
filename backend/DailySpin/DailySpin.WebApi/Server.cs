using DailySpin.Application;
using System.Net;

namespace DailySpin.WebApi;

public sealed class Server
{
    private readonly IPipeline _pipeline;

    public Server(IPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task StartAsync()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5000/");
        listener.Start();

        Console.WriteLine("Server started...");

        while (listener.IsListening)
        {
            try
            {
                var context = await listener.GetContextAsync();
                _ = ProcessRequestAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        listener.Stop();
        listener.Close();
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;

            await _pipeline.Run(response, request);

            response.OutputStream.Close();
            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while processing request: {ex.Message}");
        }
    }
}
