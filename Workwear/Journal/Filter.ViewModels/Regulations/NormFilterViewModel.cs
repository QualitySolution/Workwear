using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.ViewModels.Company;

namespace workwear.Journal.Filter.ViewModels.Regulations
{
	public class NormFilterViewModel : JournalFilterViewModelBase<NormFilterViewModel>
	{
		public NormFilterViewModel(JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<NormFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			EntryPost = builder.ForProperty(x => x.Post).MakeByType().Finish();
			EntryProtectionTools = builder.ForProperty(x => x.ProtectionTools).MakeByType().Finish();
			EntrySubdivision = builder.ForProperty(x => x.Subdivision).MakeByType().Finish();
			EntryDepartment = builder.ForProperty(x => x.Department).MakeByType().Finish();
			EntryDepartment.EntitySelector = new DepartmentJournalViewModelSelector(journal, navigation, EntrySubdivision);
		}

		#region Ограничения
		private Post post;
		public virtual Post Post {
			get => post;
			set => SetField(ref post, value);
		}

		private ProtectionTools protectionTools;
		public virtual ProtectionTools ProtectionTools {
			get => protectionTools;
			set => SetField(ref protectionTools, value);
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
					if(department!= null && !DomainHelper.EqualDomainObjects(Subdivision, department?.Subdivision))
						Subdivision = department?.Subdivision;
			}
		}
		#endregion
		#region EntityModels
		public EntityEntryViewModel<Post> EntryPost;
		public EntityEntryViewModel<ProtectionTools> EntryProtectionTools;
		public EntityEntryViewModel<Subdivision> EntrySubdivision;
		public EntityEntryViewModel<Department> EntryDepartment;

		#endregion
	}
}
