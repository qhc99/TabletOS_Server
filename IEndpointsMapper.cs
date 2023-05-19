using System.Reflection;

public interface IEndpointsMapper
{
    public void MapEndpoints(IEndpointRouteBuilder app);
}

public static class IEndpointRouteBuilderExtensions
{
    public static void MapRefelectedEndpoints(this
    IEndpointRouteBuilder app, Assembly assembly)
    {
        var endpointsMapperInterfaceType =
          typeof(IEndpointsMapper);
        var endpointsMapperTypes = assembly.GetTypes().Where(t =>
        t.IsClass &&
        !t.IsAbstract &&
        !t.IsGenericType &&
        t.GetConstructor(Type.EmptyTypes) != null &&
        endpointsMapperInterfaceType.IsAssignableFrom(t));
        foreach (var endpointsMapperType in endpointsMapperTypes)
        {
            var instantiatedType = (IEndpointsMapper)
              Activator.CreateInstance
                (endpointsMapperType)!;
            instantiatedType.MapEndpoints(app);
        }
    }
}