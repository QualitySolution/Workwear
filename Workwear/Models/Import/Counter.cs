using System;
using QS.DomainModel.Entity;

namespace workwear.Models.Import
{
	public class Counter : PropertyChangedBase
	{
		private string title;
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}

		private int count;
		public virtual int Count {
			get => count;
			set => SetField(ref count, value);
		}
	}
}
