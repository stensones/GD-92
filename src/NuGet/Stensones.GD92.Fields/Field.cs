namespace Stensones.GD92.Fields;

public abstract class Field<T> //where T:struct
{
	private T value;

    public T Value 
	{ 
		get => this.GetValue(); 
		set => this.SetValue(value); 
	}

	public Field(T initialValue)
	{
		this.value = initialValue;
	}

	public abstract void WriteToStream(Stream stream);

	protected virtual void SetValue(T value)
	{
		this.value = value; 
	}

	protected virtual T GetValue()
	{
		return this.value;
	}
}