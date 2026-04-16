using System;
using System.ComponentModel;
using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Regulations;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Regulations;

namespace Workwear.ViewModels.Company
{
	public class PostViewModel : EntityDialogViewModelBase<Post>, IDialogDocumentation
	{
		private readonly FeaturesService featuresService;
		private readonly IInteractiveQuestion interactive;

		public PostViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			FeaturesService featuresService,
			IInteractiveQuestion interactive,
			ILifetimeScope autofacScope,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
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
			Entity.PropertyChanged += Entity_PropertyChanged;
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("organization.html#posts");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		#region Controls
		public EntityEntryViewModel<Subdivision> EntrySubdivision;
		public EntityEntryViewModel<Department> EntryDepartment;
		public EntityEntryViewModel<Profession> EntryProfession;
		public EntityEntryViewModel<CostCenter> EntryCostCenter;
		#endregion

		#region Visible
		public bool VisibleCostCenter => featuresService.Available(WorkwearFeature.CostCenter);
		#endregion

		#region События
		private void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(Entity.Department)
			   && Entity.Department?.Subdivision != null
			   && Entity.Department?.Subdivision != Entity.Subdivision
			   && interactive.Question($"Изменить так же подразделение на {Entity.Department.Subdivision.Name}?")) {
				Entity.Subdivision = Entity.Department.Subdivision;
			}
		}
		#endregion
	}
}
