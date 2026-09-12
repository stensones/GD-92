using Stensones.GD92.Fields;

namespace Stensones.GD92.Messages;

public sealed record Test : IGD92MessageContents
{
	private static readonly MessageType TestMessageType =
		MessageType.FromValue(GD92MessageType.Test);

	private Test(TestType testType)
	{
		this.TestType = testType;
	}

	public TestType TestType { get; }
	public MessageType Type => TestMessageType;

	public static Test FromFields(TestType testType)
	{
		ArgumentNullException.ThrowIfNull(testType);

		return new Test(testType);
	}

	public static Test FromEncodedMessageBuffer(ref EncodedMessageBuffer buffer)
	{
		return FromFields(TestType.FromEncodedMessageBuffer(ref buffer));
	}

	public byte[] ToWireValue()
	{
		return this.TestType.ToWireValue();
	}
}
