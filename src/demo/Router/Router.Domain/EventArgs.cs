using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router.Domain;
public class EventArgs<T> : EventArgs
{
	public T Value { get; init; }

	public EventArgs(T args)
	{
		this.Value = args;
	}
}
