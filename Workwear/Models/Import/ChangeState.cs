using System;

namespace workwear.Models.Import
{
	public class ChangeState
	{
		public ChangeType ChangeType;
		public string OldValue;
		public string InterpretedValue;
		public string Warning;
		public string Error;
		public string[] WillCreatedValues;

		public ChangeState(ChangeType changeType, string oldValue = null, string interpretedValue = null, string error = null, string warning = null, string[] willCreatedValues = null)
		{
			ChangeType = changeType;
			OldValue = oldValue;
			InterpretedValue = interpretedValue;
			Warning = warning;
			Error = error;
			WillCreatedValues = willCreatedValues ?? Array.Empty<string>();
		}
	}
}
