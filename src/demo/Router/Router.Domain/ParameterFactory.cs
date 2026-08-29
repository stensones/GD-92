using Router.Domain.Fields;
using Router.Domain.Parameters;

namespace Router.Domain;

public static class ParameterFactory
{
	public static IParameter<Word8> Create<T>(string name, IField value)
	{
		return new BrigadeNumber(value as Word8 ?? new Word8(0));
	}
}