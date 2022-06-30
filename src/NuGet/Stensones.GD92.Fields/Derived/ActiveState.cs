using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stensones.GD92.Fields.Derived;
public class ActiveState
{
	//private readonly Word8 valueField;
	private bool value;

	public ActiveState(bool activeWhenOpen)
	{
		//this.valueField = new Word8(activeWhenOpen ? (byte)1 : (byte)0);
		this.value = activeWhenOpen;
	}

	public bool Value => this.value;
		//this.valueField.Value == 0 ? false : true;
}
