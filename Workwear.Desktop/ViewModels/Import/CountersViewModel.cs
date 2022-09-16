using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.ViewModels;
using Workwear.Models.Import;

namespace workwear.ViewModels.Import
{
	public class CountersViewModel : ViewModelBase
	{
		private readonly Type enumClass;

		public CountersViewModel(Type enumClass)
		{
			this.enumClass = enumClass ?? throw new ArgumentNullException(nameof(enumClass));
			foreach(Enum EnumValue in Enum.GetValues(enumClass)) {
				Counters.Add(EnumValue.ToString(), new Counter { Title = EnumValue.GetEnumTitle() });
			}
		}

		public readonly Dictionary<string, Counter> Counters = new Dictionary<string, Counter>();

		public void SetCount(Enum counter, int value)
		{
			Counters[counter.ToString()].Count = value;
		}

		public int GetCount(Enum counter)
		{
			return Counters[counter.ToString()].Count;
		}

		public void AddCount(Enum counter, int value = 1)
		{
			Counters[counter.ToString()].Count += value;
		}

		public IEnumerable<string> CountersText => Counters
			.Select(x => $"{x.Value.Title}:{x.Value.Count}");
	}
}
