using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
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
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;

namespace Workwear.ViewModels.Stock {
	public class ReturnViewModel  : EntityDialogViewModelBase<Return> {
		public ReturnViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILifetimeScope autofacScope,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
			featuresService = autofacScope.Resolve<FeaturesService>();
			
			if(featuresService.Available(WorkwearFeature.Owners))
				Owners = UoW.GetAll<Owner>().ToList();
			if(featuresService.Available(WorkwearFeature.Warehouses))
				Warhouses = UoW.GetAll<Warehouse>().ToList();
			
			var entryBuilder = new CommonEEVMBuilderFactory<Return>(this, Entity, UoW, navigation, autofacScope);
			
			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			EmployeeCardEntryViewModel = entryBuilder.ForProperty(x => x.EmployeeCard)
				.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();
		}
		
		#region Проброс свойств документа

		public virtual int DocID => Entity.Id;
		public virtual string DocTitle => Entity.Title;
		public virtual string DocComment => Entity.Comment;
		public virtual UserBase DocCreatedbyUser => Entity.CreatedbyUser;
		public virtual DateTime DocDate => Entity.Date;
		public virtual Warehouse Warehouse => Entity.Warehouse;
		public virtual EmployeeCard EmployeeCard => Entity.EmployeeCard;
		public virtual IObservableList<ReturnItem> Items => Entity.Items;

		#endregion

		#region Свойства ViewModel
		private readonly FeaturesService featuresService;
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		public readonly EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		public readonly EntityEntryViewModel<EmployeeCard> EmployeeCardEntryViewModel;
		
		public List<Owner> Owners {get;}
		public List<Warehouse> Warhouses {get;}
		
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

		#endregion 
		
		#region Свойства для View

		public virtual bool SensitiveDocNumber => !AutoDocNumber;
		public virtual bool CanAddItem => true;
		public virtual bool CanRemoveItem => false;
		public virtual bool CanSetNomenclature => false;
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
            
			Entity.UpdateOperations(UoW);
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;
			
			UoWGeneric.Save ();

			logger.Debug ("Обновляем записи о выданной одежде в карточке сотрудника...");
			Entity.UpdateEmployeeWearItems();
			
			UoWGeneric.Commit ();

			logger.Info ("Ok");
			return true;
		}

		#endregion
	}
}
