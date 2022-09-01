using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Company;

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
		}
		
		#region Ограничения
		private Subdivision subdivision;
		public Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		#endregion
		#region EntityModels
		public EntityEntryViewModel<Subdivision> EntrySubdivision;
		#endregion
		
	}
}
