using Microsoft.Extensions.DependencyInjection;

namespace Router.Domain;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddRouter(this IServiceCollection collection)
	{
		collection.AddScoped<IRouter, Router>();

		return collection;
	}
}
