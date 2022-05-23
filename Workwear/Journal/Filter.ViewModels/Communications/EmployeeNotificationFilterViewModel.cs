using System;
using System.ComponentModel.DataAnnotations;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Company;

namespace workwear.Journal.Filter.ViewModels.Communications
{
	public class EmployeeNotificationFilterViewModel : JournalFilterViewModelBase<EmployeeNotificationFilterViewModel>
	{
		#region Ограничения
		private bool showOnlyWork = true;
		public virtual bool ShowOnlyWork {
			get => showOnlyWork;
			set => SetField(ref showOnlyWork, value);
		}

		private bool showOnlyLk = true;
		public virtual bool ShowOnlyLk {
			get => showOnlyLk;
			set => SetField(ref showOnlyLk, value);
		}

		private bool showOverdue = true;
		public virtual bool ShowOverdue {
			get => showOverdue;
			set => SetField(ref showOverdue, value);
		}

		private Subdivision subdivision;
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		private AskIssueType isueType;
		public AskIssueType IsueType {
			get => isueType;
			set => SetField(ref isueType, value);
		}

		private DateTime startDateIssue;
		public DateTime StartDateIssue {
			get => startDateIssue;
			set => SetField(ref startDateIssue, value);
		}
		
		private DateTime endDateIssue;
		public DateTime EndDateIssue {
			get => endDateIssue;
			set => SetField(ref endDateIssue, value);
		}

		private bool containsPeriod;
		[PropertyChangedAlso(nameof(PeriodSensitive))]
		public bool ContainsPeriod {
			get => containsPeriod;
			set {
				if (value) ContainsDateBirthPeriod = false;
				SetField(ref containsPeriod, value);
			}
		}

		public bool PeriodSensitive => ContainsPeriod;
		public bool SensitiveDateBirth => ContainsDateBirthPeriod;
		
		private bool containsDateBirthPeriod;
		[PropertyChangedAlso(nameof(SensitiveDateBirth))]
		public bool ContainsDateBirthPeriod {
			get => containsDateBirthPeriod;
			set {
				if (value) ContainsPeriod = false;
				SetField(ref containsDateBirthPeriod, value);
			}
		}

		private DateTime startDateBirth;
		public DateTime StartDateBirth {
			get => startDateBirth;
			set => SetField(ref startDateBirth, value);
		}
		
		private DateTime endDateBirth;
		public DateTime EndDateBirth {
			get => endDateBirth;
			set => SetField(ref endDateBirth, value);
		}

		#endregion

		#region EntityModels

		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
		#endregion

		public EmployeeNotificationFilterViewModel(JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<EmployeeNotificationFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			SubdivisionEntry = builder.ForProperty(x => x.Subdivision)
				.MakeByType()
				.Finish();
			startDateIssue = startDateBirth = DateTime.Today;
			endDateIssue = endDateBirth = startDateIssue.AddDays(14);
		}
	}
	public enum AskIssueType
	{
		[Display(Name = "Все")]
		All,
		[Display(Name = "Персональная")]
		Personal,
		[Display(Name = "Коллективная")]
		Сollective
	}
}
