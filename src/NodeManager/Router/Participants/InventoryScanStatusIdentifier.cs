namespace NodeManager.Router.Participants;

public sealed class InventoryScanStatusIdentifier
{
	private InventoryScanStatusIdentifier(Guid value)
	{
		this.Value = value;
	}

	public Guid Value { get; }

	public static InventoryScanStatusIdentifier Create()
	{
		return new InventoryScanStatusIdentifier(Guid.CreateVersion7());
	}

	public static bool TryParse(
		string? value,
		out InventoryScanStatusIdentifier? identifier)
	{
		if (Guid.TryParse(value, out var parsedValue))
		{
			identifier = new InventoryScanStatusIdentifier(parsedValue);
			return true;
		}

		identifier = null;
		return false;
	}

	public override string ToString()
	{
		return this.Value.ToString();
	}
}
