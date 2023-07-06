using System;
using System.ComponentModel;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Stock;

namespace Workwear.ViewModels.Regulations
{
	public class ProtectionToolsViewModel : EntityDialogViewModelBase<ProtectionTools>
	{
		private readonly ILifetimeScope autofacScope;
		private readonly IInteractiveService interactiveService;

		public ProtectionToolsViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation, ILifetimeScope autofacScope, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			var entryBuilder = new CommonEEVMBuilderFactory<ProtectionTools>(this, Entity, UoW, navigation, autofacScope);
			ItemTypeEntryViewModel = entryBuilder.ForProperty(x => x.Type)
			.MakeByType()
			.Finish();

			Entity.ObservableNomenclatures.ListContentChanged += (sender, args) => OnPropertyChanged(nameof(SensitiveCreateNomenclature));
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
		#endregion

		#region EntityViewModels
		public readonly EntityEntryViewModel<ItemsType> ItemTypeEntryViewModel;
		#endregion

		#region Действия View
		#region Аналоги
		public void AddAnalog()
		{
			if(Entity.Type == null) {
				interactiveService.ShowMessage(ImportanceLevel.Error, "Не указан тип номенклатуры!");
				return;
			}
			var page = NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel, ItemsType>(
				this, Entity.Type, 
				OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += Analog_OnSelectResult;
		}

		void Analog_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var toolsNode in e.SelectedObjects) {
				var tools = UoW.GetById<ProtectionTools>(toolsNode.GetId());
				if (tools.Type == Entity?.Type)
					Entity.AddAnalog(tools);
			}
		}

		public void RemoveAnalog(ProtectionTools[] tools)
		{
			foreach(var item in tools) {
				Entity.RemoveAnalog(item);
			}
		}
		#endregion
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
				Entity.AddNomeclature(nomenclature);
			}
		}

		public void RemoveNomenclature(Nomenclature[] tools)
		{
			foreach(var item in tools) {
				Entity.RemoveNomeclature(item);
			}
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
			Entity.AddNomeclature(nomenclature);
		}

		#endregion
		#endregion
	}
}
