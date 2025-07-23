using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Company;
using Workwear.Domain.Visits;
using workwear.Journal.ViewModels.Company;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;

namespace Workwear.ViewModels.Visits {
	public class VisitViewModel: EntityDialogViewModelBase<Visit>, IDialogDocumentation
	{
		public VisitViewModel(
			BaseParameters baseParameters,
			FeaturesService featuresService,
			ILifetimeScope autofacScope, 
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null
		) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			
			var entryBuilder = new CommonEEVMBuilderFactory<Visit>(this, Entity, UoW, navigation, autofacScope);

			EmployeeCardEntryViewModel = entryBuilder.ForProperty(x => x.Employee)
				.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();
			EmployeeCardEntryViewModel.IsEditable = CanEditEmployee;
		}

		private readonly BaseParameters baseParameters;
		private readonly FeaturesService featuresService;
		public readonly EntityEntryViewModel<EmployeeCard> EmployeeCardEntryViewModel;
		
		public string DocumentationUrl { get; }
		public string ButtonTooltip { get; }

		#region Проброс свойств
		
		public virtual DateTime CreateDate { get => Entity.CreateDate; set => Entity.CreateDate = value;}
		public virtual DateTime VisitDate { get => Entity.VisitTime; set => Entity.VisitTime = value;}
		public virtual EmployeeCard Employee { get => Entity.Employee; set => Entity.Employee = value;}
		public virtual bool Cancelled { get => Entity.Cancelled; set => Entity.Cancelled = value;}
		public virtual bool Done { get => Entity.Done; set => Entity.Done = value;}
		public virtual string Comment { get => Entity.Comment; set => Entity.Comment = value;}

		#endregion

		#region CanEdit

		public virtual bool CanEditVisitDate => true;
		public virtual bool CanEditEmployee => true;
		
		#endregion
	}
}
