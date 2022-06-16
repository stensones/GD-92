namespace Stensones.GD92.Fields;

public abstract class Field<T> //where T:struct
{
    public T Value { get; set; }

	public Field(T initialValue)
	{
		this.Value = initialValue;
	}
}