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

		public virtual TDataTypeEnum DataTypeEnum =>
			dataType?.Data is TDataTypeEnum ? (TDataTypeEnum)dataType.Data : default;

		private DataType dataType;
		[PropertyChangedAlso(nameof(DataTypeEnum))]
		public DataType DataType {
			get => dataType;
			set {
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
