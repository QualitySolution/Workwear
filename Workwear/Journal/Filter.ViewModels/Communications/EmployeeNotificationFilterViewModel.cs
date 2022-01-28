using System;
using System.ComponentModel.DataAnnotations;
using Autofac;
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
			startDateIssue = DateTime.Today;
			endDateIssue = startDateIssue.AddDays(14);
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
