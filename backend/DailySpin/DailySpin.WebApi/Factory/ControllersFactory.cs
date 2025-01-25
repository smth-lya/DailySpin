using DailySpin.DI;

namespace DailySpin.WebApi;

public sealed class ControllersFactory : IControllersFactory
{
    private readonly IScope _scope;

    public ControllersFactory(IScope scope)
    {
        _scope = scope;
    }

    public object CreateController(Type controllerType)
    {
        return ActivatorUtilities.GetServiceOrCreateInstance(_scope, controllerType);
    }
}
public interface IControllersFactory
{
    object CreateController(Type controllerType);
}