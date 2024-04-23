using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Stock 
{
	public class IssueBySubstituteFundViewModel : EntityDialogViewModelBase<SubstituteFundDocuments> 
	{
		public FeaturesService FeaturesService { get; }
		public SizeService SizeService { get; }
		
		public IssueBySubstituteFundViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation,
			IUserService userService, FeaturesService featuresService, SizeService sizeService,
			IValidator validator = null, UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder,
			unitOfWorkFactory, navigation, validator, unitOfWorkProvider) 
		{
			FeaturesService = featuresService;
			SizeService = sizeService;
			if (UoW.IsNew) 
			{
				Entity.CreatedbyUser = userService.GetCurrentUser();
			}
		}
		
		#region Commands
		public void SelectNomenclatureFromEmployee() 
		{
			var selectJournal = 
				NavigationManager.OpenViewModel<EmployeeBalanceJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.DateSensitive = false;
			selectJournal.ViewModel.Filter.CheckShowWriteoffVisible = false;
			//selectJournal.ViewModel.Filter.SubdivisionSensitive =  Employee == null;
			//selectJournal.ViewModel.Filter.EmployeeSensitive = Employee == null;
			selectJournal.ViewModel.Filter.Date = Entity.Date;
			//selectJournal.ViewModel.Filter.CanChooseAmount = true;
			selectJournal.ViewModel.OnSelectResult += AddNomenclatureFromEmployee;
		}
		
		private void AddNomenclatureFromEmployee(object sender, JournalSelectedEventArgs e) 
		{
			IList<EmployeeIssueOperation> operations = 
				UoW.GetById<EmployeeIssueOperation>(e.GetSelectedObjects<EmployeeBalanceJournalNode>().Select(x => x.Id));

			foreach(EmployeeIssueOperation empOp in operations) 
			{
				SubstituteFundOperation subsOp = new SubstituteFundOperation() 
				{
					EmployeeIssueOperation = empOp,
					OperationTime = DateTime.Now
				};
				
				Entity.AddItem(subsOp);
			}
		}

		public void SelectSubtituteNomenclature(SubstituteFundDocumentItem item)
		{
			IPage<StockBalanceJournalViewModel> selectJournal = 
				MainClass.MainWin.NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(this, OpenPageOptions.AsSlave);

			selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectJournal.ViewModel.Filter.OnlyWithBarcode = true;
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Single;
			selectJournal.ViewModel.Filter.ItemsType = item.SubstituteFundOperation.EmployeeIssueOperation.Nomenclature.Type;
			_ = selectJournal.ViewModel.Filter.ItemsType.Name;
			bool b = selectJournal.ViewModel.Filter.ItemsType == item.SubstituteFundOperation.EmployeeIssueOperation.Nomenclature.Type;
			selectJournal.Tag = item;
			selectJournal.ViewModel.OnSelectResult += AddSubtituteNomenclature;
		}
		
		public void AddSubtituteNomenclature(object sender, JournalSelectedEventArgs e) 
		{
			//TODO
			/*
			 При первой подмене происхоидт обновление записи
			 (поялвяется операция выдачи и операция по складу)
			 далее создаются новые операции по штрихкодам.
			 Создать сервис подменного фонда, который будет получать последнию операцию по штрихкоду (для определениеи где он),
			 создавать новую запись и т.д. Это нужно, так как необходимо позже внедрять подменный фонд			 
			 */
			
			StockBalanceJournalNode node = e.GetSelectedObjects<StockBalanceJournalNode>().First();
			/*var a= UoW.GetAll<Barcode>()
				.Where(b => b.Nomenclature.Id == node.NomeclatureId &&
				            b.Size.Id == node.SizeId && b.Height.Id == node.HeightId)
				.Where(b => b.BarcodeOperations.).ToList();*/
			Barcode barcode = UoW.GetAll<Barcode>()
				.Where(b => b.Nomenclature.Id == node.NomeclatureId && 
				            b.Size.Id == node.SizeId && b.Height.Id == node.HeightId)
				.FirstOrDefault();

			if (barcode == null) return;
			IPage page = NavigationManager.FindPage((StockBalanceJournalViewModel)sender);
			SubstituteFundDocumentItem item = page.Tag as SubstituteFundDocumentItem;
			//item.StockPosition = node.GetStockPosition(UoW);

			//TODO
			//CalculateTotal();
		}
		
		public void DeleteItem(SubstituteFundDocumentItem item) 
		{
			Entity.DeleteItem(item);
		}
		#endregion

		#region Save
		public override bool Save() 
		{
			return base.Save();
		}
		#endregion
	}
}
