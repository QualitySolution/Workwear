using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using Workwear.Tools.Features;
using Workwear.ViewModels.Regulations;

namespace Workwear.ViewModels.Company
{
	public class PostViewModel : EntityDialogViewModelBase<Post>
	{
		private readonly FeaturesService featuresService;
		private readonly ILifetimeScope autofacScope;

		public PostViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			FeaturesService featuresService,
			ILifetimeScope autofacScope,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var builder = new CommonEEVMBuilderFactory<Post>(this, Entity, UoW, NavigationManager, autofacScope);

			EntrySubdivision = builder.ForProperty(x => x.Subdivision)
									.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
									.UseViewModelDialog<SubdivisionViewModel>()
									.Finish();

			EntryDepartment = builder.ForProperty(x => x.Department)
									.UseViewModelJournalAndAutocompleter<DepartmentJournalViewModel>()
									.UseViewModelDialog<DepartmentViewModel>()
									.Finish();

			EntryProfession = builder.ForProperty(x => x.Profession)
									.UseViewModelJournalAndAutocompleter<ProfessionJournalViewModel>()
									.UseViewModelDialog<ProfessionViewModel>()
									.Finish();

			EntryCostCenter = builder.ForProperty(x => x.CostCenter)
									.MakeByType()
									.Finish();
		}

		#region Controls

		public EntityEntryViewModel<Subdivision> EntrySubdivision;
		public EntityEntryViewModel<Department> EntryDepartment;
		public EntityEntryViewModel<Profession> EntryProfession;
		public EntityEntryViewModel<CostCenter> EntryCostCenter;

		#endregion

		#region Visible

		public bool VisibleCostCenter => featuresService.Available(WorkwearFeature.CostCenter);

		#endregion
	}
}
