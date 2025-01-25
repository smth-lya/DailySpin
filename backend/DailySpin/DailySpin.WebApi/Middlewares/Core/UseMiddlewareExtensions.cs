using DailySpin.DI;
using System.Reflection;

namespace DailySpin.WebApi;

public static class UseMiddlewareExtensions
{
    public static IPipelineBuilder UseMiddleware<TMiddleware>(this IPipelineBuilder builder, params object?[] args)
    {
        return builder.UseMiddleware(typeof(TMiddleware), args);
    }

    public static IPipelineBuilder UseMiddleware(this IPipelineBuilder builder, Type middleware, params object?[] args)
    {
        if (typeof(IMiddleware).IsAssignableFrom(middleware))
        {
            // IMiddleware doesn't support passing args directly since it's
            // activated from the container
            if (args.Length > 0)
            {
                throw new NotSupportedException();
            }

            var interfaceBinder = new InterfaceMiddlewareBinder(middleware, builder.Scope);
            return builder.Use(interfaceBinder.CreateMiddleware);
        }

        var factory = new MiddlewareFactory(builder.Scope, middleware, args);
        builder.Use(factory.CreateMiddleware);

        return builder;
    }
    public class InterfaceMiddlewareBinder
    {
        private readonly Type _middlewareType;
        private readonly IScope _scope;

        public InterfaceMiddlewareBinder(Type middlewareType, IScope scope)
        {
            _middlewareType = middlewareType;
            _scope = scope;
        }

        public RequestDelegate CreateMiddleware(RequestDelegate next)
        {
            return async context =>
            {
                var middleware = _scope.Resolve(_middlewareType) as IMiddleware;
                
                if (middleware == null)
                {
                    throw new InvalidOperationException();
                }

                try
                {
                    await middleware.InvokeAsync(context, next);
                }
                finally
                {

                }
            };
        }

        public override string ToString() => _middlewareType.ToString();
    }

    private sealed class MiddlewareFactory
    {
        internal const string InvokeMethodName = "Invoke";
        internal const string InvokeAsyncMethodName = "InvokeAsync";

        private readonly IScope _scope;
        private readonly Type _middleware;

        private readonly object?[] _args;
        private readonly MethodInfo _invokeMethod;
        private readonly ParameterInfo[] _parameters;

        public MiddlewareFactory(IScope scope, Type middleware, object?[] args)
        {
            _args = args;
            _scope = scope;
            _middleware = middleware;

            var methods = middleware.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            
            MethodInfo? invokeMethod = null;

            foreach (var method in methods)
            {
                if (string.Equals(method.Name, InvokeMethodName, StringComparison.Ordinal) ||
                    string.Equals(method.Name, InvokeAsyncMethodName, StringComparison.Ordinal))
                {
                    if (invokeMethod is not null)
                    {
                        throw new InvalidOperationException("Найдено несколько методов Invoke/InvokeAsync.");
                    }

                    invokeMethod = method;
                }
            }

            if (invokeMethod is null)
            {
                throw new InvalidOperationException("Метод Invoke или InvokeAsync не найден в middleware.");
            }

            if (!typeof(Task).IsAssignableFrom(invokeMethod.ReturnType))
            {
                throw new InvalidOperationException("Метод Invoke/InvokeAsync должен возвращать Task.");
            }

            _invokeMethod = invokeMethod;

            var parameters = invokeMethod.GetParameters();
            if (parameters.Length == 0 || parameters[0].ParameterType != typeof(HttpContext)) 
            {
                throw new InvalidOperationException($"Первым параметром метода Invoke/InvokeAsync должен быть {nameof(HttpContext)}");
            }

            _parameters = parameters;
        }

        public RequestDelegate CreateMiddleware(RequestDelegate next)
        {
            var ctorArgs = new object[_parameters.Length + 1];
            ctorArgs[0] = next;
            Array.Copy(_args, 0, ctorArgs, 1, _args.Length);
            var instance = ActivatorUtilities.CreateInstance(_scope, _middleware, ctorArgs);

            if (_parameters.Length == 2)
            {
                return (RequestDelegate)_invokeMethod.CreateDelegate(typeof(RequestDelegate), instance);
            }

            throw new NotImplementedException();
        }
    }

}
