using Microsoft.Extensions.DependencyInjection;
using Moq;
using Router.Domain;

namespace Router.Application.Tests.Unit;

public class ServiceCollectionExtensionsShould
{
    [Fact]
    public void RegisterZeroInterfaces()
    {
		var seriveProvider = new Mock<IServiceCollection>();

		var actual = seriveProvider.Object.AddRouter();

		seriveProvider.Verify(
			_ => _.Add(It.IsAny<ServiceDescriptor>()),
			Times.Never());
	}
}