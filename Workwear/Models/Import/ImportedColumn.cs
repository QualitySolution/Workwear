using System;
using QS.DomainModel.Entity;

namespace workwear.Models.Import
{
	public class ImportedColumn<TDataTypeEnum> : PropertyChangedBase, IDataColumn
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

		Enum IDataColumn.DataType => dataType;

		public ImportedColumn(int index)
		{
			Index = index;
		}
	}

	public interface IDataColumn
	{
		string Title { get; }
		Enum DataType { get; }
	}
}
