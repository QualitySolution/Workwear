using System;

namespace workwear.Models.Import
{
	public class ChangeState
	{
		public ChangeType ChangeType;
		public string OldValue;
		public string InterpretedValue;
		public string[] WillCreatedValues;

		public ChangeState(ChangeType changeType, string oldValue = null, string interpretedValue = null, string[] willCreatedValues = null)
		{
			ChangeType = changeType;
			OldValue = oldValue;
			InterpretedValue = interpretedValue;
			WillCreatedValues = willCreatedValues ?? Array.Empty<string>();
		}
	}
}
