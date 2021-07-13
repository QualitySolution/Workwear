using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace workwear.Models.Import
{
	public class ImportedColumn<TDataTypeEnum> : PropertyChangedBase
		where TDataTypeEnum : System.Enum
	{
		public readonly int Index;

		private string title;
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}

		private TDataTypeEnum dataType;
		public virtual TDataTypeEnum DataType {
			get => dataType;
			set => SetField(ref dataType, value);
		}

		public ImportedColumn(int index)
		{
			Index = index;
		}
	}
}
