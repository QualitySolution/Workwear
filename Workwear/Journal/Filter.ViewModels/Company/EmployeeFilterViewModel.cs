using System;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company;

namespace workwear.Journal.Filter.ViewModels.Company
{
	public class EmployeeFilterViewModel : JournalFilterViewModelBase<EmployeeFilterViewModel>
	{
		#region Ограничения

		private bool showOnlyWork = true;
		public virtual bool ShowOnlyWork {
			get => showOnlyWork;
			set => SetField(ref showOnlyWork, value);
		}

		private Subdivision subdivision;
		public virtual Subdivision Subdivision {
			get => subdivision;
			set {
				SetField(ref subdivision, value);
				if(Department != null && Department.Subdivision != Subdivision)
					Department = null;
			}
		}

		private Department department;
		public virtual Department Department {
			get => department;
			set {
				if(SetField(ref department, value))
					if(!DomainHelper.EqualDomainObjects(Subdivision, department?.Subdivision))
						Subdivision = department?.Subdivision;
			}
		}


		#endregion

		#region EntityModels

		public EntityEntryViewModel<Subdivision> SubdivisionEntry;

		public EntityEntryViewModel<Department> DepartmentEntry;

		#endregion

		public EmployeeFilterViewModel(JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<EmployeeFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			SubdivisionEntry = builder.ForProperty(x => x.Subdivision)
				.MakeByType()
				.Finish();

			DepartmentEntry = builder.ForProperty(x => x.Department)
				.MakeByType()
				.Finish();
			DepartmentEntry.EntitySelector = new DepartmentJournalViewModelSelector(journal, navigation, SubdivisionEntry);
		}
	}
}
