using DailySpin.Application;
using DailySpin.Domain;
using DailySpin.Infrastructure;
using DailySpin.DI;
using DailySpin.ORM;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Reflection;

namespace DailySpin.WebApi;

public class Program
{
    public static async Task Main()
    {
        var builder = new ContainerBuilder(new LambdaBasedActiovationBuilder());
        var container = builder
            .RegisterSingleton<Assembly>(typeof(Program).Assembly)
            .RegisterSingleton<JwtOptions>(new JwtOptions()
            {
                SecretKey = "3ryls3CxvVVvlhY7ifDQdBd6adqE9Cj0",
                Issuer = "daylyapp.com",
                Audience = "client-app",
            })
            .RegisterSingleton<IJwtEncoder, JwtEncoder>()

            .RegisterSingleton<IControllersFactory>((scope) => new ControllersFactory(scope))

            .RegisterScoped<StaticFileMiddleware, StaticFileMiddleware>()
            .RegisterScoped<EndpointRoutingMiddleware, EndpointRoutingMiddleware>()
            .RegisterScoped<AuthenticationMiddleware, AuthenticationMiddleware>()
            .RegisterScoped<AuthorizationMiddleware, AuthorizationMiddleware>()
            .RegisterScoped<EndpointMiddleware, EndpointMiddleware>()

            .RegisterSingleton<IJwtService, JwtService>()
            .RegisterSingleton<IJwtRefreshTokenStorage, CacheRefreshTokenStorage>()

            .RegisterSingleton<UserRepository, UserRepository>()
            .RegisterSingleton<IUserReadRepository>((scope) => scope.Resolve<UserRepository>())
            .RegisterSingleton<IUserRepository>((scope) => scope.Resolve<UserRepository>())

            .RegisterSingleton<IUserService, UserService>()
            .RegisterSingleton<IUserBusinessRulePredicates, DefaultUserBusinessRulePredicates>()

            .RegisterSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()))
            .RegisterSingleton<IPasswordHasher, DefaultPasswordHasher>()
            .RegisterSingleton<ICustomDbConnection>((scope) =>
            {
                var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=2546;Database=dailyApp");
                connection.Open();

                return connection.Adapt();
            })
            .RegisterSingleton<ApplicationDbContext, ApplicationDbContext>()
            
            .RegisterSingleton<SignUpEndpoint, SignUpEndpoint>()
            .RegisterSingleton<SignInEndpoint, SignInEndpoint>()
            .RegisterSingleton<SignOutEndpoint, SignOutEndpoint>()
            
            .RegisterSingleton<RefreshEndpoint, RefreshEndpoint>()
            .RegisterSingleton<ValidateEndpoint, ValidateEndpoint>()

            .RegisterSingleton<UserController, UserController>()
            .RegisterSingleton<WheelController, WheelController>()

            .Build();

        var scope = container.CreateScope();

        var pipeline = new MiddlewarePipeline.MiddlewarePipelineBuilder(scope)
            .UseStaticFile()
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints()
            .Build();

        var server = new Server(pipeline);
        await server.StartAsync();
    }
}

// <iframe