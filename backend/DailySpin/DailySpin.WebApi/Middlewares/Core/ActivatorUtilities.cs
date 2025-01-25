using DailySpin.DI;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace DailySpin.WebApi;

internal static class ActivatorUtilities
{
    public static object CreateInstance(IScope scope, Type instanceType, params object[] parameters)
    {
        var bestLength = -1;

        ConstructorMatcher bestMather = default;

        if (!instanceType.IsAbstract)
        {
            foreach (var constructor in instanceType.GetConstructors())
            {
                var matcher = new ConstructorMatcher(constructor);
                var length = matcher.Match(parameters);

                if (bestLength < length)
                {
                    bestLength = length;
                    bestMather = matcher;
                }
            }
        }

        if (bestLength == -1)
        {
            throw new InvalidOperationException($"Не удалось найти подходящий конструктор для типа '{instanceType}'. Убедитесь, что тип конкретный, и зарегистрируйте зависимости для всех параметров конструктора.");
        }

        return bestMather.CreateInstance(scope);
    }

    public static T CreateInstance<T>(IScope scope, params object[] parameters)
    {
        return (T)CreateInstance(scope, typeof(T), parameters);
    }

    public static T GetServiceOrCreateInstance<T>(IScope scope)
    {
        return (T)GetServiceOrCreateInstance(scope, typeof(T));
    }

    public static object GetServiceOrCreateInstance(IScope scope, Type type)
    {
        return scope.Resolve(type) ?? CreateInstance(scope, type);
    }

    private struct ConstructorMatcher
    {
        private readonly ConstructorInfo _constructor;
        private readonly ParameterInfo[] _parameters;
        private readonly object?[] _parameterValues;

        public ConstructorMatcher(ConstructorInfo constructor)
        {
            _constructor = constructor;
            _parameters = _constructor.GetParameters();
            _parameterValues = new object?[_parameters.Length];
        }

        public int Match(object[] givenParameters)
        {
            var applyIndexStart = 0;
            var applyExactLength = 0;

            for (int givenIndex = 0; givenIndex < givenParameters.Length; givenIndex++)
            {
                var givenType = givenParameters[givenIndex]?.GetType();
                var givenMatched = false;

                for (int applyIndex = applyIndexStart; givenMatched == false && applyIndex < _parameters.Length; applyIndex++)
                {
                    if (_parameterValues[applyIndex] == null &&
                        _parameters[applyIndex].ParameterType.IsAssignableFrom(givenType))
                    {
                        givenMatched = true;
                        _parameterValues[applyIndex] = givenParameters[givenIndex];
                    
                        if (applyIndex == applyIndexStart)
                        {
                            applyIndexStart++;

                            if (applyIndex == givenIndex)
                            {
                                applyExactLength = applyIndex;
                            }
                        }
                    }
                }

                if (givenMatched == false)
                {
                    return -1;
                }
            }

            return applyExactLength;
        }

        public object CreateInstance(IScope scope)
        {
            for (int index = 0; index < _parameters.Length; index++)
            {
                var parameter = _parameters[index];

                if (_parameterValues[index] is null)
                {
                    var value = scope.Resolve(parameter.ParameterType);
                    if (value is null)
                    {
                        throw new InvalidOperationException($"Не удалось разрешить зависимость для параметра '{_parameters[index].Name}' типа '{_parameters[index].ParameterType}'.");
                    }

                    _parameterValues[index] = value;   
                }
            }

            try
            {
                return _constructor.Invoke(_parameterValues);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                // Приведенная выше строка всегда будет генерироваться, но компилятор требует, чтобы мы генерировали ее явно.
                throw;
            }
        }
    }
}