using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Analytics;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Regulations
{
	public class ProtectionToolsViewModel : EntityDialogViewModelBase<ProtectionTools>
	{
		private readonly IInteractiveService interactiveService;
		private readonly FeaturesService featuresService;

		public ProtectionToolsViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			FeaturesService featuresService,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			var entryBuilder = new CommonEEVMBuilderFactory<ProtectionTools>(this, Entity, UoW, navigation, autofacScope);
			ItemTypeEntryViewModel = entryBuilder.ForProperty(x => x.Type)
				.MakeByType()
				.Finish();
			
			CategoriesEntryViewModel = entryBuilder.ForProperty(x => x.CategoryForAnalytic)
				.MakeByType()
				.Finish();

			Entity.Nomenclatures.CollectionChanged += EntityNomenclaturesChanged;
			Entity.PropertyChanged += EntityOnPropertyChanged;
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

		#region Visible
		public bool VisibleSaleCost => featuresService.Available(WorkwearFeature.Selling);
		public bool ShowSupply => featuresService.Available(WorkwearFeature.StockForecasting);
		public bool ShowSupplyUnisex => SupplyType == SupplyType.Unisex;
		public bool ShowSupplyTwosex => SupplyType == SupplyType.TwoSex;
		#endregion

		#region EntityViewModels
		public readonly EntityEntryViewModel<ItemsType> ItemTypeEntryViewModel;
		public readonly EntityEntryViewModel<ProtectionToolsCategory> CategoriesEntryViewModel;
		#endregion

		#region Проброс свойств
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

		public void RemoveNomenclature(Nomenclature[] tools)
		{
			foreach(var item in tools) {
				Entity.RemoveNomenclature(item);
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
		#endregion
	}
}
