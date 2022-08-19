using System;
using System.ComponentModel;
using System.Linq;
using QS.DomainModel.Entity;

namespace workwear.Models.Import
{
	public class ExcelColumn : PropertyChangedBase
	{
		public readonly int Index;

		private string title;
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}

		public ExcelValueTarget[] DataTypeByLevels { get; }

		public ExcelColumn(int index, int levels)
		{
			Index = index;
			DataTypeByLevels = Enumerable.Range(0, levels).Select(x => new ExcelValueTarget(this, x)).ToArray();
			foreach(var level in DataTypeByLevels) {
				level.PropertyChanged += (sender, args) =>  OnPropertyChanged(nameof(DataTypeByLevels));
			}
		}
	}
}
