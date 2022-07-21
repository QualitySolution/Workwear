using System;
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
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Regulations;
using workwear.Journal.ViewModels.Stock;
using Workwear.Measurements;

namespace workwear.ViewModels.Regulations
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

			Entity.ObservableNomenclatures.ListContentChanged += (sender, args) => OnPropertyChanged(nameof(SensetiveCreateNomenclature));
		}

		#region EntityViewModels
		public EntityEntryViewModel<ItemsType> ItemTypeEntryViewModel;
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
		public void AddNomeclature()
		{
			if(Entity.Type == null) {
				interactiveService.ShowMessage(ImportanceLevel.Error, "Не указан тип номенклатуры!");
				return;
			}
			var selectPage = NavigationManager.OpenViewModel<NomenclatureJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectPage.ViewModel.Filter.ItemType = Entity.Type; 
			selectPage.ViewModel.Filter.EntryItemsType.IsEditable = false;
			selectPage.ViewModel.JournalFilter.IsShow = false;
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectPage.ViewModel.OnSelectResult += Nomeclature_OnSelectResult;
		}

		void Nomeclature_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			var nomenclatures = UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()));
			foreach(var nomenclature in nomenclatures) {
				if (nomenclature.Type == Entity?.Type)
					Entity.AddNomeclature(nomenclature);
			}
		}

		public void RemoveNomeclature(Nomenclature[] tools)
		{
			foreach(var item in tools) {
				Entity.RemoveNomeclature(item);
			}
		}

		public bool SensetiveCreateNomenclature => !Entity.Nomenclatures.Any(x => x.Name == Entity.Name);

		public void CreateNomeclature()
		{
			var nomenclaure = new Nomenclature {
				Name = Entity.Name,
				Comment = Entity.Comment,
				Type = Entity.Type,
				Sex = ClothesSex.Universal,
			};
			Entity.AddNomeclature(nomenclaure);
		}

		#endregion
		#endregion
	}
}
