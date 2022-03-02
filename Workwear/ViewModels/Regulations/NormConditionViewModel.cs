using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Regulations;

namespace workwear.ViewModels.Regulations
{
	public class NormConditionViewModel : EntityDialogViewModelBase<NormCondition>
	{
		public NormConditionViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory,
			navigation, validator) {
		}
		public List<int> StartDays => DayInMonth(SelectedStartMonth);
		public List<int> EndDays => DayInMonth(SelectedEndMonth);

		private int? startDay;
		public int? StartDay {
			get => startDay;
			set => startDay = value;
		}
		private int? endDay;
		public int? EndDay {
			get => endDay;
			set => endDay = value;
		}
		private DateTime? selectedStartMonth;
		[PropertyChangedAlso(nameof(StartDays))]
		public DateTime? SelectedStartMonth {
			get => selectedStartMonth;
			set => SetField(ref selectedStartMonth, value);
		}
		private DateTime? selectedEndMonth;
		[PropertyChangedAlso(nameof(EndDays))]
		public DateTime? SelectedEndMonth { 
			get => selectedEndMonth;
			set => SetField(ref selectedEndMonth, value);
		}
		private DateTime[] months;
		public DateTime[] Months => months ?? (months = AllMonths());
		private static DateTime[] AllMonths() {
			return new[] {
				new DateTime(2001, 1, 1),
				new DateTime(2001, 2, 1),
				new DateTime(2001, 3, 1),
				new DateTime(2001, 4, 1),
				new DateTime(2001, 5, 1),
				new DateTime(2001, 6, 1),
				new DateTime(2001, 7, 1),
				new DateTime(2001, 8, 1),
				new DateTime(2001, 9, 1),
				new DateTime(2001, 10, 1),
				new DateTime(2001, 11, 1),
				new DateTime(2001, 12, 1)};
		}
		private static List<int> DayInMonth(DateTime? date) {
			var days = new List<int>();
			if (date is null)
				return days;
			for (int i = 1; i < DateTime.DaysInMonth(date.Value.Year, date.Value.Month) + 1; i++) { days.Add(i);}
			return days;
		}
		public override bool Save() {
			if (SelectedStartMonth != null)
				Entity.IssuanceStart = new DateTime(2001, SelectedStartMonth.Value.Month, StartDay ?? 1);
			else
				Entity.IssuanceStart = null;
			if (SelectedEndMonth != null)
				Entity.IssuanceEnd = new DateTime(2001, SelectedEndMonth.Value.Month, EndDay ?? 1);
			else
				Entity.IssuanceEnd = null;

			if (!Validate()) return false;
			UoW.Save(Entity); return true;
		}
	}
}