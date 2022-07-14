using System;
using System.ComponentModel;
using QS.DomainModel.Entity;

namespace workwear.Models.Import
{
	public class ImportedColumn<TDataTypeEnum> : PropertyChangedBase, IDataColumn
		where TDataTypeEnum : Enum
	{
		public readonly int Index;

		private string title;
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}

		private TDataTypeEnum dataTypeEnum;
		public virtual TDataTypeEnum DataTypeEnum {
			get => dataTypeEnum;
			set => SetField(ref dataTypeEnum, value);
	}

		private DataType dataType;
		public DataType DataType {
			get => dataType;
			set {
				if (value.Data is TDataTypeEnum dataTypeEnum)
					DataTypeEnum = dataTypeEnum;
				SetField(ref dataType, value);
			}
		}

		public ImportedColumn(int index)
		{
			Index = index;
		}
	}

	public interface IDataColumn : INotifyPropertyChanged
	{
		string Title { get; }
		DataType DataType { get; set; }
	}
}
