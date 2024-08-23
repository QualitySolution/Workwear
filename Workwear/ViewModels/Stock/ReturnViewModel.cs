using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;

namespace Workwear.ViewModels.Stock {
	public class ReturnViewModel  : EntityDialogViewModelBase<Return> {
		public ReturnViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			StockRepository stockRepository,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null,
			EmployeeCard employee = null,
			Warehouse warehouse = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
			featuresService = autofacScope.Resolve<FeaturesService>();
			
			if(featuresService.Available(WorkwearFeature.Owners))
				owners = UoW.GetAll<Owner>().ToList();
			if(Entity.Id == 0) {
				Entity.EmployeeCard = employee;
				Entity.Warehouse = warehouse ?? stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			}
			
			var entryBuilder = new CommonEEVMBuilderFactory<Return>(this, Entity, UoW, navigation, autofacScope);
			
			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			
			EmployeeCardEntryViewModel = entryBuilder.ForProperty(x => x.EmployeeCard)
				.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();
			EmployeeCardEntryViewModel.PropertyChanged += EmployeeCardEntryViewModelPropertyChanged;
			CanEditEmployee = Entity.Id == 0;
			EmployeeCardEntryViewModel.IsEditable = CanEditEmployee;
		}

		private void EmployeeCardEntryViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(EmployeeCardEntryViewModel.Entity))
				OnPropertyChanged(nameof(CanEditItems));
		}

		#region Проброс свойств документа

		public virtual int DocID => Entity.Id;
		public virtual string DocTitle => Entity.Title;
		public virtual UserBase DocCreatedbyUser => Entity.CreatedbyUser;
		public virtual string DocComment { get => Entity.Comment; set => Entity.Comment = value;}
		public virtual EmployeeCard EmployeeCard => Entity.EmployeeCard;
		public virtual DateTime DocDate { get => Entity.Date;set => Entity.Date = value;}
		public virtual IObservableList<ReturnItem> Items => Entity.Items;
		#endregion

		#region Свойства ViewModel
		private readonly FeaturesService featuresService;
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		public readonly EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		public readonly EntityEntryViewModel<EmployeeCard> EmployeeCardEntryViewModel;
		
		private List<Owner> owners = new List<Owner>();
		public List<Owner> Owners => owners;
		
		private bool autoDocNumber = true;
		[PropertyChangedAlso(nameof(DocNumberText))]
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		public virtual bool AutoDocNumber {
			get => autoDocNumber;
			set => SetField(ref autoDocNumber, value);
		}

		public virtual string DocNumberText {
			get => AutoDocNumber ? (Entity.Id != 0 ? Entity.Id.ToString() : "авто" ) : Entity.DocNumber;
			set => Entity.DocNumber = (AutoDocNumber || value == "авто") ? null : value;
		}
		
		private ReturnItem selectedItem;
		[PropertyChangedAlso(nameof(CanRemoveItem))]
		[PropertyChangedAlso(nameof(CanSetNomenclature))]
		public virtual ReturnItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}

		#endregion 
		
		#region Свойства для View

		public bool CanEditEmployee;// => DocID == 0;
		public virtual bool SensitiveDocNumber => !AutoDocNumber;
		public virtual bool CanAddItem => true;
		public virtual bool CanRemoveItem => SelectedItem != null;
		public virtual bool CanSetNomenclature => SelectedItem != null;
		public virtual bool CanEditItems => EmployeeCard != null;
		public virtual bool OwnersVisible => featuresService.Available(WorkwearFeature.Owners);
		public virtual bool WarehouseVisible => featuresService.Available(WorkwearFeature.Exchange1C);
		#endregion

		#region Методы

		public void AddFromEmployee() {
			var selectJournal = 
				NavigationManager.OpenViewModel<EmployeeBalanceJournalViewModel, EmployeeCard>(
					this,
					EmployeeCard,
					OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.DateSensitive = false;
			selectJournal.ViewModel.Filter.CheckShowWriteoffVisible = false;
			selectJournal.ViewModel.Filter.SubdivisionSensitive = false;
			selectJournal.ViewModel.Filter.EmployeeSensitive = false;
			selectJournal.ViewModel.Filter.Date = Entity.Date;
			selectJournal.ViewModel.Filter.CanChooseAmount = false;
			selectJournal.ViewModel.OnSelectResult += SelectFromEmployee_Selected;
		}
		private void SelectFromEmployee_Selected(object sender, JournalSelectedEventArgs e) {
			var operations = UoW.GetById<EmployeeIssueOperation>(e.GetSelectedObjects<EmployeeBalanceJournalNode>().Select(x => x.Id));
			var addedAmount = ((EmployeeBalanceJournalViewModel)sender).Filter.AddAmount;
			var balance = e.GetSelectedObjects<EmployeeBalanceJournalNode>().ToDictionary(k => k.Id, v => v.Balance);

			foreach (var operation in operations)
				Entity.AddItem(operation, addedAmount == AddedAmount.One ? 1 : (addedAmount == AddedAmount.Zero ? 0 : balance[operation.Id])); 
//CalculateTotal(null, null);
		}
		public void DeleteItem(ReturnItem item) {
			Entity.RemoveItem(item); 
			OnPropertyChanged(nameof(CanRemoveItem));
			OnPropertyChanged(nameof(CanSetNomenclature));
//CalculateTotal(null, null);
		}
		
		public void SetNomenclature(ReturnItem item) {
			//var selectJournal = MainClass.MainWin.NavigationManager
			//	.OpenViewModelOnTdi<NomenclatureJournalViewModel>(MyTdiDialog, OpenPageOptions.AsSlave);
			var selectJournal = NavigationManager.OpenViewModel<NomenclatureJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.ProtectionTools = item.IssuedEmployeeOnOperation?.ProtectionTools; 
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Single;
			selectJournal.ViewModel.OnSelectResult += (s, je) =>  SetNomenclatureSelected(item,je.SelectedObjects.First().GetId());
		}

		private void SetNomenclatureSelected(ReturnItem item, int selectedId) {
			item.Nomenclature = UoW.GetById<Nomenclature>(selectedId);
		}
		
		public override bool Save() {
			logger.Info ("Запись документа...");
            
//Валидация ???			
			
			Entity.UpdateOperations(UoW);
			if (Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;

			if(!base.Save()) {
            	logger.Info("Не Ок.");
            	return false;
            }

			logger.Debug ("Обновляем записи о выданной одежде в карточке сотрудника...");
			Entity.UpdateEmployeeWearItems(UoW);

			UoW.Commit();
			
			logger.Info ("Ok");
			return true;
		}
		#endregion
		
	}
}
