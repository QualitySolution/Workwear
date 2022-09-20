using System;
using System.Linq;

namespace Workwear.Models.Import
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

		#region Расчетные
		public bool NeedSave => ChangeType == ChangeType.ChangeValue || ChangeType == ChangeType.NewEntity;
		#endregion

		public void AddCreatedValues(string value) {
			var list = WillCreatedValues.ToList();
			list.Add(value);
			WillCreatedValues = list.ToArray();
		}
	}
}
