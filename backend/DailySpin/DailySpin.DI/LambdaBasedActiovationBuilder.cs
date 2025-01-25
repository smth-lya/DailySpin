using System.Linq.Expressions;
using System.Reflection;

namespace DailySpin.DI;

public interface IActivationBuilder
{
    Func<IScope, object> BuildActivation(ServiceDescriptor descriptor);
}

public abstract class BasedActivationBuilder : IActivationBuilder
{
    public Func<IScope, object> BuildActivation(ServiceDescriptor descriptor)
    {
        var tb = (TypeBasedServiceDescriptor)descriptor;

        var ctor = tb.ImplementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single();
        var args = ctor.GetParameters();

        return BuildActivationInternal(tb, ctor, args, descriptor);
    }

    protected abstract Func<IScope, object> BuildActivationInternal(TypeBasedServiceDescriptor tb, ConstructorInfo ctor, ParameterInfo[] args, ServiceDescriptor descriptor);
}

public class LambdaBasedActiovationBuilder : BasedActivationBuilder
{
    private static readonly MethodInfo ResolveMethod = typeof(IScope).GetMethods()
        .FirstOrDefault(m => m.Name == "Resolve" && !m.IsGenericMethod)!;

    protected override Func<IScope, object> BuildActivationInternal(TypeBasedServiceDescriptor tb, ConstructorInfo ctor, ParameterInfo[] args, ServiceDescriptor descriptor)
    {
        var scopeParameter = Expression.Parameter(typeof(IScope), "scope");

        var ctorArgs = args.Select(x =>
            Expression.Convert(Expression.Call(scopeParameter, ResolveMethod,
                Expression.Constant(x.ParameterType)), x.ParameterType));
        var @new = Expression.New(ctor, ctorArgs);

        var lambda = Expression.Lambda<Func<IScope, object>>(@new, scopeParameter);
        return lambda.Compile();
    }
}