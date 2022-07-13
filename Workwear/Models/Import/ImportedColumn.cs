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

		private TDataTypeEnum dataType;
		public virtual TDataTypeEnum DataType {
			get => dataType;
			set => SetField(ref dataType, value);
	}

		private EntityField entityField;
		public EntityField EntityField {
			get => entityField;
			set {
				if (value.Data is TDataTypeEnum dataTypeEnum)
					DataType = dataTypeEnum;
				SetField(ref entityField, value);
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
		EntityField EntityField { get; set; }
	}
}
