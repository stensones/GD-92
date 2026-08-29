using Microsoft.Extensions.DependencyInjection;

namespace Stensones.GD92.Fields;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddProtocolField(this IServiceCollection services)
	{
		services.AddSingleton<ISerializableProtocolField, ProtocolField>();
		return services;
	}
}