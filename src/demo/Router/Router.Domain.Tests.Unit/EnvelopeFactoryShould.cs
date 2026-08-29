using Moq;
using Router.Domain.Fields;
using Router.Domain.Messages;

namespace Router.Domain.Tests.Unit;

public class EnvelopeFactoryShould
{
	private readonly CommsAddress source;
	private readonly List<CommsAddress> destinations;
	private readonly Mock<IMessage> message;

	public EnvelopeFactoryShould()
	{
		this.source = new CommsAddress(26, 1, 10);
		this.destinations = new List<CommsAddress>
		{
			new CommsAddress(26, 1, 10)
		};
		this.message = new Mock<IMessage>();
	}

	[Fact]
	public void Create_WithValidParameters_ReturnsNewEnvelope()
	{
		var envelope = EnvelopeFactory.Create(this.source, this.destinations, this.message.Object);
		
		Assert.NotNull(envelope);
	}

	[Fact]
	public void Set_Source_In_Created_Envelope()
	{ 
		var envelope = EnvelopeFactory.Create(this.source, this.destinations, this.message.Object);

		Assert.Equal(this.source, envelope.Source);
	}

	[Fact]
	public void Set_Destinations_In_Created_Envelope()
	{
		var envelope = EnvelopeFactory.Create(this.source, this.destinations, this.message.Object);

		Assert.Equal(this.destinations, envelope.Destinations);
	}

	[Fact]
	public void Set_Message_In_Created_Envelope()
	{
		var envelope = EnvelopeFactory.Create(this.source, this.destinations, this.message.Object);

		Assert.Equal(this.message.Object, envelope.Message);
	}
}
