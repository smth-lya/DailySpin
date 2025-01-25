namespace DailySpin.DI;

public abstract class ServiceDescriptor
{
    public Type ServiceType { get; init; }
    public Lifetime Lifetime { get; init; }
}

/// <summary>
/// Для самой простой нашей реализации, когда говорим, что у нас есть Iservice и для него создавай экземпляры Service. Как контейнер это будет делать - неважно
/// </summary>
public class TypeBasedServiceDescriptor : ServiceDescriptor
{
    public Type ImplementationType { get; init; }
}

/// <summary>
/// Когда передаем сюда делегат
/// </summary>
public class FactoryBasedServiceDescriptor : ServiceDescriptor
{
    public Func<IScope, object> Factory { get; init; }
}

/// <summary>
/// Когда передаем сюда инстанс (экземпляр) и он может быть только сингтон 
/// </summary>
public class InstanceBasedServiceDescriptor : ServiceDescriptor
{
    public object Instance { get; init; }

    public InstanceBasedServiceDescriptor(Type serviceType, object instance)
    {
        Lifetime = Lifetime.Singleton;
        ServiceType = serviceType;
        Instance = instance;
    }
}
