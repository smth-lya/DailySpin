namespace DailySpin.DI;

public enum Lifetime
{
    Transient,
    Scoped,
    Singleton
}

public interface IContainerBuilder
{
    void Register(ServiceDescriptor descriptor);
    IContainer Build();
}

public interface IContainer : IDisposable, IAsyncDisposable
{
    IScope CreateScope();
}

public interface IScope : IDisposable, IAsyncDisposable
{
    object Resolve(Type service);

    TService Resolve<TService>();
}