using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace EcoCode.Tool.Library.Infrastructure;

internal sealed class TypeRegistrar(ServiceCollection builder) : ITypeRegistrar
{
    public ITypeResolver Build() => new TypeResolver(builder.BuildServiceProvider());

    public void Register(Type service, Type implementation) => builder.AddSingleton(service, implementation);

    public void RegisterInstance(Type service, object implementation) => builder.AddSingleton(service, implementation);

    public void RegisterLazy(Type service, Func<object> func) => builder.AddSingleton(service, _ => func());
}
