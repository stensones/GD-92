using Microsoft.Extensions.DependencyInjection;

namespace Stensones.GD92.Fields;

public static class ServiceCollectionExtensions
{
	extension(IServiceCollection services)
	{
		public IServiceCollection AddGD92Fields()
		{
			services.AddSingleton<ISerializableProtocolField, ProtocolField>();
			return services;
		}
	}
}