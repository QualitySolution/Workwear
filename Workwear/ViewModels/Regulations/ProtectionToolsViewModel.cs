using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Analytics;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Company;
using Workwear.Repository.Operations;
using Workwear.Repository.Regulations;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Regulations
{
	public class ProtectionToolsViewModel : EntityDialogViewModelBase<ProtectionTools>, IDialogDocumentation
	{
		private readonly IInteractiveService interactiveService;
		private readonly FeaturesService featuresService;
		private readonly NormRepository normRepository;
		private readonly EmployeeRepository employeeRepository;

		public ProtectionToolsViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			FeaturesService featuresService,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			NormRepository normRepository,
			EmployeeRepository employeeRepository,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.normRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			var entryBuilder = new CommonEEVMBuilderFactory<ProtectionTools>(this, Entity, UoW, navigation, autofacScope);
			ItemTypeEntryViewModel = entryBuilder.ForProperty(x => x.Type)
				.MakeByType()
				.Finish();
			
			CategoriesEntryViewModel = entryBuilder.ForProperty(x => x.CategoryForAnalytic)
				.MakeByType()
				.Finish();

			sizeChangeRestriction = Entity.SizeChangeRestriction != null;
			
			Entity.ProtectionToolsNomenclatures.CollectionChanged += EntityNomenclaturesChanged;
			Entity.PropertyChanged += EntityOnPropertyChanged;
		}
		
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("regulations.html#protection-tools");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion

		private IList<EmployeeIssueOperation> usedOperations;
		private IList<EmployeeIssueOperation> UsedOperations {
			get {//Устанавливаем только при первом обращении
				if(usedOperations == null) {
					if(Entity.Id == 0)
						usedOperations = new List<EmployeeIssueOperation>();
					else {
						var repo = new EmployeeIssueRepository();
						usedOperations = repo.AllOperationsFor(protectionTools: new[] { Entity }, uow: UoW);
					}
				}
				return usedOperations;
			}
		}

		#region События
		private void EntityOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch(e.PropertyName) {
				case nameof(Entity.Name):
				case nameof(Entity.Type):
					OnPropertyChanged(nameof(SensitiveCreateNomenclature));
					break;
			}
		}
		
		private void EntityNomenclaturesChanged(object sender, NotifyCollectionChangedEventArgs e) {
			OnPropertyChanged(nameof(SensitiveCreateNomenclature));
			if(!Entity.Nomenclatures.Any(n => DomainHelper.EqualDomainObjects(n, Entity.SupplyNomenclatureUnisex)))
				ClearSupplyNomenclatureUnisex();
			if(!Entity.Nomenclatures.Any(n => DomainHelper.EqualDomainObjects(n, Entity.SupplyNomenclatureMale)))
				ClearSupplyNomenclatureMale();
			if(!Entity.Nomenclatures.Any(n => DomainHelper.EqualDomainObjects(n, Entity.SupplyNomenclatureFemale)))
				ClearSupplyNomenclatureFemale();
		}
		#endregion

		#region Visible Sensitive
		public bool VisibleSaleCost => featuresService.Available(WorkwearFeature.Selling);
		public bool VisibleChoosingNomenclature => featuresService.Available(WorkwearFeature.EmployeeChoose);
		public bool ShowSupply => featuresService.Available(WorkwearFeature.StockForecasting) || featuresService.Available(WorkwearFeature.ExportExcel);
		public bool ShowSupplyUnisex => SupplyType == SupplyType.Unisex;
		public bool ShowSupplyTwosex => SupplyType == SupplyType.TwoSex;
		public bool ShowCategoryForAnalytics => featuresService.Available(WorkwearFeature.Dashboard);
		public bool SensitiveDispenser => Entity.DermalPpe;
		public bool CanUseSizeRestriction => featuresService.Available(WorkwearFeature.EmployeeLk);
		public bool CanSetDayOfSizeRestriction => SizeChangeRestriction;
		
		#endregion

		#region EntityViewModels
		public readonly EntityEntryViewModel<ItemsType> ItemTypeEntryViewModel;
		public readonly EntityEntryViewModel<ProtectionToolsCategory> CategoriesEntryViewModel;
		#endregion

		#region Cвойства VM и проброс из Entyty
		public virtual SupplyType SupplyType {
			get => Entity.SupplyType;
			set {
				if(Entity.SupplyType == value)
					return;
				Entity.SupplyType = value;
				OnPropertyChanged(nameof(ShowSupplyUnisex));
				OnPropertyChanged(nameof(ShowSupplyTwosex));
			}
		}

		public virtual bool Dispenser {
			get => Entity.Dispenser;
			set {
				if(Entity.Dispenser == value)
					return;
				if(value && UsedOperations.Count != 0) {
					interactiveService.ShowMessage(ImportanceLevel.Warning,
						$"По этой номенклатуре нормы уже совершено {UsedOperations.Count} выдач." +
						$" Её нельзя перевести в выдаваемую дозатором - создайте новую.");
					OnPropertyChanged();
					return;
				}

				Entity.Dispenser = value;
			}
		}

		public virtual bool WashingPPE {
			get => Entity.DermalPpe;
			set {
				if(Entity.DermalPpe == value)
					return;
				Entity.DermalPpe = value;
				if(!Entity.DermalPpe)
					Entity.Dispenser = false;
				OnPropertyChanged(nameof(SensitiveDispenser));
			}
		}

		private bool sizeChangeRestriction;
		public virtual bool SizeChangeRestriction {
			get => sizeChangeRestriction;
			set { if(sizeChangeRestriction != value) {
					sizeChangeRestriction = value;
					if(value)
						Entity.SizeChangeRestriction = DayOfSizeRestriction;
					else
						Entity.SizeChangeRestriction = null;
					OnPropertyChanged(nameof(CanSetDayOfSizeRestriction));
				}
			}
		}
		
		public virtual int DayOfSizeRestriction {
			get => Entity.SizeChangeRestriction ?? 0;
			set { if(SizeChangeRestriction && Entity.SizeChangeRestriction != value) {
					Entity.SizeChangeRestriction = value;
					OnPropertyChanged();
				}
			}
		}

		public virtual bool Archival {
			get => Entity.Archival;
			set {
				if(Entity.Archival == value)
					return;
				Entity.Archival = value;
				if(!UpdateWorkwearItems())
					Entity.Archival = !Entity.Archival; // Откат, т.к. выше поле заведомо было изменено.
				OnPropertyChanged();
			}
		}
		#endregion
		#region Действия View
		#region Номеклатуры
		public void AddNomenclature()
		{
			var selectPage = NavigationManager.OpenViewModel<NomenclatureJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectPage.ViewModel.Filter.ItemType = Entity.Type;
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectPage.ViewModel.OnSelectResult += Nomenclature_OnSelectResult;
		}

		void Nomenclature_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var nomenclatures = UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()));
			foreach(var nomenclature in nomenclatures) {
				Entity.AddNomenclature(nomenclature);
			}
		}

		public void RemoveNomenclature(ProtectionToolsNomenclature[] items)
		{
			foreach(var item in items) {
				Entity.RemoveNomenclature(item.Nomenclature);
			}
		}

		public void ClearSupplyNomenclatureUnisex() {
			Entity.SupplyNomenclatureUnisex = null;
		}

		public void ClearSupplyNomenclatureMale() {
			Entity.SupplyNomenclatureMale = null;
		}

		public void ClearSupplyNomenclatureFemale() {
			Entity.SupplyNomenclatureFemale = null;
		}

		public bool SensitiveCreateNomenclature => !String.IsNullOrWhiteSpace(Entity.Name)
			&& Entity.Type != null
			&& Entity.Nomenclatures.All(x => x.Name != Entity.Name);

		public void CreateNomenclature()
		{
			var nomenclature = new Nomenclature {
				Name = Entity.Name,
				Comment = Entity.Comment,
				Type = Entity.Type,
				Sex = ClothesSex.Universal,
			};
			UoW.Save(nomenclature);
			Entity.AddNomenclature(nomenclature);
		}

		public void OpenNomenclature(Nomenclature nomenclature) {
			NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(nomenclature.Id));
		}
		#endregion

		#region Обновление потребностей

		public bool UpdateWorkwearItems() {
			var norms = normRepository.GetNormsUseProtectionTools(Entity.Id, UoW).ToArray(); 
			var employees = employeeRepository.GetEmployeesUseNorm(norms, UoW, false);
			if(employees.Any()) {
				string message = $"После отключения поменяются потребности у {employees.Count} сотрудников. Сохранить изменения?";
				if(interactiveService.Question(message)) {
					var progressPage = NavigationManager.OpenViewModel<ProgressWindowViewModel>(null);
					var progress = progressPage.ViewModel.Progress;
					progress.Start(employees.Count, text: "Обновляем потребности сотрудников");
					foreach(var employee in employees) {
						employee.UoW = UoW;
						employee.UpdateWorkwearItems();
						UoW.Save(employee);
					}
					NavigationManager.ForceClosePage(progressPage, CloseSource.FromParentPage);
				} else
					return false;
			}
			return true;
		}

		#endregion
		#endregion
	}
}
