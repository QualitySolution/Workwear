using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.ViewModels.Company;

namespace workwear.Journal.Filter.ViewModels.Company {
	public class PostFilterViewModel  : JournalFilterViewModelBase<PostFilterViewModel>
	{
		public PostFilterViewModel(JournalViewModelBase journal, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			IUnitOfWorkFactory unitOfWorkFactory = null) : base(journal, unitOfWorkFactory) 
		{
			var builder = new CommonEEVMBuilderFactory<PostFilterViewModel>(journal, this, UoW, navigation, autofacScope);
			EntrySubdivision = builder.ForProperty(x => x.Subdivision).MakeByType().Finish();
			EntryDepartment = builder.ForProperty(x => x.Department).MakeByType().Finish();
			EntryDepartment.EntitySelector = new DepartmentJournalViewModelSelector(journal, navigation, EntrySubdivision);
		}
		
		#region Ограничения
		private Subdivision subdivision;
		public Subdivision Subdivision {
			get => subdivision;
			set {
				SetField(ref subdivision, value);
				if(Department != null && Department.Subdivision != Subdivision)
					Department = null;
			}
		}

		private Department department;
		public Department Department {
			get => department;
			set {
				SetField(ref department, value);
				if(Department != null && Department.Subdivision != null)
					Subdivision = Department.Subdivision;
			}
		}

		#endregion
		#region EntityModels
		public EntityEntryViewModel<Subdivision> EntrySubdivision;
		public EntityEntryViewModel<Department> EntryDepartment;
		#endregion
		
	}
}
