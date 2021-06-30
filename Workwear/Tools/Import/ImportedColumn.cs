using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace workwear.Tools.Import
{
	public class ImportedColumn : PropertyChangedBase
	{
		public readonly int Index;

		private string title;
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}

		private DataType dataType;
		public virtual DataType DataType {
			get => dataType;
			set => SetField(ref dataType, value);
		}

		public ImportedColumn(int index)
		{
			Index = index;
		}
	}
}
