using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Company;

namespace workwear.Journal.Filter.ViewModels.Tools
{
	public class NotificationFilterViewModel: JournalFilterViewModelBase<NotificationFilterViewModel>
	{
		#region Ограничения
		private bool showOnlyOverdue = true;
		public virtual bool ShowOnlyOverdue {
			get => showOnlyOverdue;
			set => SetField(ref showOnlyOverdue, value);
		}

		private Subdivision subdivision;
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}
		#endregion

		#region EntityModels

		public EntityEntryViewModel<Subdivision> SubdivisionEntry;

		#endregion

		public NotificationFilterViewModel(JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<NotificationFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			SubdivisionEntry = builder.ForProperty(x => x.Subdivision)
				.MakeByType()
				.Finish();
		}
	}
}
