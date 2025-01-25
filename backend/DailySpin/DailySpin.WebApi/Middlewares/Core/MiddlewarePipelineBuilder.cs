using DailySpin.DI;
using System.Net;

namespace DailySpin.WebApi;

public class MiddlewarePipeline : IPipeline
{
    private readonly RequestDelegate _pipeline;

    private MiddlewarePipeline(RequestDelegate pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task Run(HttpListenerResponse response, HttpListenerRequest request)
    {
        var context = new HttpContext() 
        {
            Request = request, 
            Response = response 
        };
        
        await _pipeline.Invoke(context);
    }

    public class MiddlewarePipelineBuilder : IPipelineBuilder
    {
        private readonly List<Func<RequestDelegate, RequestDelegate>> _middlewares = new();

        public IScope Scope { get; init; }

        public MiddlewarePipelineBuilder(IScope scope)
        {
            Scope = scope;
        }

        public IPipelineBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }

        public IPipeline Build()
        {
            RequestDelegate pipeline = context =>
            {
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            };

            foreach (var middleware in _middlewares.AsEnumerable().Reverse())
            {
                pipeline = middleware(pipeline);
            }

            return new MiddlewarePipeline(pipeline);
        }
    }
}



public interface IPipelineBuilder
{
    IPipelineBuilder Use(Func<RequestDelegate, RequestDelegate> middleware);
    IPipeline Build();

    IScope Scope { get; }
}

public interface IPipeline
{
    Task Run(HttpListenerResponse response, HttpListenerRequest request);
}
