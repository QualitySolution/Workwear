using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Company;

namespace workwear.Journal.Filter.ViewModels.Regulations
{
	public class NormFilterViewModel : JournalFilterViewModelBase<NormFilterViewModel>
	{
		public NormFilterViewModel(JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<NormFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			PostEntry = builder.ForProperty(x => x.Post)
				.MakeByType()
				.Finish();
		}

		private Post post;
		public virtual Post Post {
			get => post;
			set => SetField(ref post, value);
		}
		#region EntityModels

		public EntityEntryViewModel<Post> PostEntry;
		#endregion
	}
}
