namespace DailySpin.DI;

public static class ContainerBuilderExtensions
{
    private static IContainerBuilder RegisterType(this IContainerBuilder builder, Type service, Type implementation, Lifetime lifetime)
    {
        builder.Register(new TypeBasedServiceDescriptor()
        {
            ImplementationType = implementation,
            ServiceType = service,
            Lifetime = lifetime
        });
        return builder;
    }

    private static IContainerBuilder RegisterFactory(this IContainerBuilder builder, Type service, Func<IScope, object> factory, Lifetime lifetime)
    {
        builder.Register(new FactoryBasedServiceDescriptor()
        {
            ServiceType = service,
            Factory = factory,
            Lifetime = lifetime
        });
        return builder;
    }

    private static IContainerBuilder RegisterInstance(this IContainerBuilder builder, Type service, object instance)
    {
        builder.Register(new InstanceBasedServiceDescriptor(service, instance));
        return builder;
    }

    public static IContainerBuilder RegisterSingleton(this IContainerBuilder builder, Type servieInterface, Type serviceImplementation)
        => builder.RegisterType(servieInterface, serviceImplementation, Lifetime.Singleton);

    public static IContainerBuilder RegisterTransient(this IContainerBuilder builder, Type servieInterface, Type serviceImplementation)
        => builder.RegisterType(servieInterface, serviceImplementation, Lifetime.Transient);

    public static IContainerBuilder RegisterScoped(this IContainerBuilder builder, Type servieInterface, Type serviceImplementation)
        => builder.RegisterType(servieInterface, serviceImplementation, Lifetime.Scoped);

    public static IContainerBuilder RegisterSingleton<TService, TImplementation>(this IContainerBuilder builder) where TImplementation : TService
    => builder.RegisterType(typeof(TService), typeof(TImplementation), Lifetime.Singleton);

    public static IContainerBuilder RegisterTransient<TService, TImplementation>(this IContainerBuilder builder) where TImplementation : TService
        => builder.RegisterType(typeof(TService), typeof(TImplementation), Lifetime.Transient);

    public static IContainerBuilder RegisterScoped<TService, TImplementation>(this IContainerBuilder builder) where TImplementation : TService
        => builder.RegisterType(typeof(TService), typeof(TImplementation), Lifetime.Scoped);


    public static IContainerBuilder RegisterSingleton(this IContainerBuilder builder, Type service, Func<IScope, object> factory)
        => builder.RegisterFactory(service, factory, Lifetime.Singleton);
  
    public static IContainerBuilder RegisterTransient(this IContainerBuilder builder, Type service, Func<IScope, object> factory)
        => builder.RegisterFactory(service, factory, Lifetime.Transient);
    
    public static IContainerBuilder RegisterScoped(this IContainerBuilder builder, Type service, Func<IScope, object> factory)
        => builder.RegisterFactory(service, factory, Lifetime.Scoped);

    public static IContainerBuilder RegisterSingleton<TService>(this IContainerBuilder builder, Func<IScope, TService> factory)
    => builder.RegisterFactory(typeof(TService), s => factory(s)!, Lifetime.Singleton);

    public static IContainerBuilder RegisterTransient<TService>(this IContainerBuilder builder, Func<IScope, TService> factory)
        => builder.RegisterFactory(typeof(TService), s => factory(s)!, Lifetime.Transient);

    public static IContainerBuilder RegisterScoped<TService>(this IContainerBuilder builder, Func<IScope, TService> factory)
        => builder.RegisterFactory(typeof(TService), s => factory(s)!, Lifetime.Scoped);


    public static IContainerBuilder RegisterSingleton(this IContainerBuilder builder, Type service, object instance) 
        => builder.RegisterInstance(service, instance);

    public static IContainerBuilder RegisterSingleton<TService>(this IContainerBuilder builder, object instance) 
        => builder.RegisterInstance(typeof(TService), instance);
}