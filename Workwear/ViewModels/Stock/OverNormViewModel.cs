using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Domain.Users;
using workwear.Journal.ViewModels.Company;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Barcodes;
using Workwear.Tools.OverNorms;
using Workwear.Tools.OverNorms.Models;

namespace Workwear.ViewModels.Stock 
{
	public sealed class OverNormViewModel : EntityDialogViewModelBase<OverNorm> 
	{
		private readonly IOverNormFactory overNormFactory;
		private readonly BarcodeService barcodeService;
		
		public EntityEntryViewModel<Warehouse> EntryWarehouseViewModel { get; }

		private OverNormModelBase overNormModel; 
		public OverNormModelBase OverNormModel { get => overNormModel; set => SetField(ref overNormModel, value); } 
		
		public OverNormViewModel(IEntityUoWBuilder uowBuilder,
			ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation, IUserService userService,
			IOverNormFactory overNormFactory, BarcodeService barcodeService,
			IValidator validator = null, UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) 
		{
			if (autofacScope == null) throw new ArgumentNullException(nameof(autofacScope));
			this.overNormFactory = overNormFactory ?? throw new ArgumentNullException(nameof(overNormFactory));
			this.barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
			
			foreach (OverNormItem item in Entity.Items)
			{
				OverNormOperation operation = item.OverNormOperation;
				item.Param =
					new OverNormParam(operation.Employee, operation.WarehouseOperation.Nomenclature, operation.WarehouseOperation.Amount, operation.WarehouseOperation.WearSize,
						operation.WarehouseOperation.Height, operation.EmployeeIssueOperation, operation.BarcodeOperations.Select(x => x.Barcode).ToList());
			}
			
			var builder = new CommonEEVMBuilderFactory<OverNorm>(this, Entity, UoW, NavigationManager, autofacScope);
			EntryWarehouseViewModel = builder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			EntryWarehouseViewModel.Changed += (sender, args) => 
			{
				OnPropertyChanged(nameof(CanAddItems));
			};
			
			OverNormModel = overNormFactory.CreateModel(UoW, Entity.Type);
			if (Entity.Id < 1)
			{
				Entity.CreatedbyUser = userService.GetCurrentUser();
			}
			
			Entity.Items.ContentChanged += CalculateTotal;
			CalculateTotal(null, null);
		}

		#region View Properties
		public bool CanAddItems => Entity.Warehouse != null && OverNormModel.Editable;
		
		public OverNormType DocType 
		{
			get => Entity.Type;
			set 
			{
				if (Entity.Type == value) return;
				Entity.Type = value;
				Entity.Items.Clear();
				OverNormModel = overNormFactory.CreateModel(UoW, value);
			}
		}
		
		public bool SensitiveDocNumber => !AutoDocNumber;
		
		private string total;
		public string Total 
		{
			get => total;
			set => SetField(ref total, value);
		}
		
		private bool autoDocNumber = true;

		[PropertyChangedAlso(nameof(DocNumber))]
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		public bool AutoDocNumber { get => autoDocNumber; set => SetField(ref autoDocNumber, value); }
		public string DocNumber 
		{
			get => AutoDocNumber ? (Entity.Id != 0 ? Entity.Id.ToString() : "авто" ) : Entity.DocNumber;
			set => Entity.DocNumber = (AutoDocNumber || value == "авто") ? null : value;
		}
		#endregion

		private void CalculateTotal(object sender, EventArgs eventArgs)
		{
			Total = $"Позиций в документе: {Entity.Items.Count}  " +
			        $"Количество единиц: {Entity.Items.Sum(x => x.OverNormOperation.WarehouseOperation?.Amount)}";
		}
		
		#region Commands

		#region Substitute
		public void SelectEmployeeIssue() 
		{
			IPage<EmployeeBalanceJournalViewModel> selectJournal = 
				NavigationManager.OpenViewModel<EmployeeBalanceJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.DateSensitive = false;
			selectJournal.ViewModel.Filter.CheckShowWriteoffVisible = false;
			selectJournal.ViewModel.Filter.Date = Entity.Date;
			selectJournal.ViewModel.Filter.EmployeeSensitive = true;
			selectJournal.ViewModel.Filter.AddAmount = AddedAmount.One;
			selectJournal.ViewModel.OnSelectResult += AddNomenclatureFromEmployee;
		}
		
		private void AddNomenclatureFromEmployee(object sender, JournalSelectedEventArgs e) 
		{
			IList<EmployeeIssueOperation> operations = 
				UoW.GetById<EmployeeIssueOperation>(e.GetSelectedObjects<EmployeeBalanceJournalNode>().Select(x => x.Id));

			foreach (EmployeeIssueOperation empOp in operations) 
			{
				if (Entity.Items.Any(x => x.OverNormOperation.EmployeeIssueOperation.Id == empOp.Id)) continue;
				Entity.AddItem(new OverNormOperation() { Employee = empOp.Employee, EmployeeIssueOperation = empOp });
			}
		}
		#endregion

		#region Guest and Repair
		public void SelectEmployee() 
		{
			IPage<EmployeeJournalViewModel> selectJournal = NavigationManager.OpenViewModel<EmployeeJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Single;
			selectJournal.ViewModel.OnSelectResult += AddEmployee;
		}
		
		private void AddEmployee(object sender, JournalSelectedEventArgs e) 
		{
			IList<EmployeeCard> employees = 
				UoW.GetById<EmployeeCard>(e.GetSelectedObjects<EmployeeJournalNode>().Select(x => x.Id));
			foreach (EmployeeCard employee in employees) 
			{
				Entity.AddItem(new OverNormOperation() { Employee = employee });
			}
		}
		#endregion
		
		public void SelectNomenclature(OverNormItem item)
		{
			IPage<StockBalanceJournalViewModel> selectJournal = 
				NavigationManager.OpenViewModel<StockBalanceJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.ShowWithBarcodes = OverNormModel.UseBarcodes;
			selectJournal.ViewModel.Filter.CanChangeShowWithBarcodes = OverNormModel.CanChangeUseBarcodes;
			selectJournal.ViewModel.Filter.Warehouse = Entity.Warehouse;
			selectJournal.ViewModel.Filter.WarehouseEntry.IsEditable = false;
			selectJournal.ViewModel.Filter.AddAmount = AddedAmount.One;
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Single;
			
			if (OverNormModel.RequiresEmployeeIssueOperation) 
			{
				selectJournal.ViewModel.Filter.ItemsType = item.OverNormOperation.EmployeeIssueOperation.Nomenclature.Type;
			}

			selectJournal.Tag = item;
			selectJournal.ViewModel.OnSelectResult += AddNomenclature;
		}
		
		public void AddNomenclature(object sender, JournalSelectedEventArgs e) 
		{
			StockBalanceJournalNode node = e.GetSelectedObjects<StockBalanceJournalNode>().First();
			OverNormModel.UseBarcodes = ((StockBalanceJournalViewModel)sender).Filter.ShowWithBarcodes;
			var nomenclature = UoW.GetById<Nomenclature>(node.NomeclatureId);
			Size size = node.SizeId != null ? UoW.GetById<Size>((int)node.SizeId) : null;
			Size height = node.HeightId != null ? UoW.GetById<Size>((int)node.HeightId) : null;
//1289			
//var size = UoW.GetById<Size>(node.SizeIdn);
//var height = UoW.GetById<Size>(node.HeightIdn);

			Barcode barcode = null;
			IPage page = NavigationManager.FindPage((StockBalanceJournalViewModel)sender);
			OverNormItem item = (OverNormItem)page.Tag;
			if (OverNormModel.UseBarcodes) 
			{
				IPage<BarcodeJournalViewModel> barcodeJournal = NavigationManager.OpenViewModel<BarcodeJournalViewModel>(this, OpenPageOptions.AsSlave);
				barcodeJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
				barcodeJournal.ViewModel.Nomenclature = nomenclature;
				barcodeJournal.ViewModel.Size = size;
				barcodeJournal.ViewModel.Height = height;
				barcodeJournal.ViewModel.OnlyFreeBarcodes = true;
				barcodeJournal.ViewModel.Warehouse = Entity.Warehouse;
				barcodeJournal.ViewModel.OnSelectResult += (o, args) => 
				{
					IList<BarcodeJournalNode> nodes = args.GetSelectedObjects<BarcodeJournalNode>();
					IList<Barcode> barcodes = UoW.GetById<Barcode>(nodes.Select(x => x.Id));
					item.Param =
						new OverNormParam(item.OverNormOperation.Employee, nomenclature, barcodes.Count, size, height, item.OverNormOperation.EmployeeIssueOperation, barcodes);
					
					AddOrUpdateItem(item);
				};
				
				return;
			}
			
			item.Param =
				new OverNormParam(item.OverNormOperation.Employee, nomenclature, 1, size, height, item.OverNormOperation.EmployeeIssueOperation);
			AddOrUpdateItem(item);
		}

		public void DeleteItem(OverNormItem item) 
		{
			Entity.DeleteItem(item);
		}

		public void DeleteBarcodeFromItem(OverNormItem item, Barcode barcode) 
		{
			if (item.Param.Barcodes.Count == 1) 
			{
				DeleteItem(item);
				return;
			}

			OverNormModel.UseBarcodes = true;
			item.Param.Barcodes.Remove(barcode);
			AddOrUpdateItem(item);
		}
		#endregion

		#region Save
		public override bool Save() 
		{
			if (!Validate()) 
			{
				return false;
			}

			foreach(OverNormItem item in Entity.Items)
			{
				UoW.Save(item.OverNormOperation.WarehouseOperation);
				UoW.Save(item.OverNormOperation);
				foreach(BarcodeOperation bo in item.OverNormOperation.BarcodeOperations) 
				{
					UoW.Save(bo);
				}
			}
			
			return base.Save();
		}
		#endregion
		
		private void AddOrUpdateItem(OverNormItem item)
		{
			if (Entity.Id < 1 || item.Id < 1) 
			{
				Entity.Items.Remove(item);
				OverNormModel.AddOperation(Entity, item.Param, Entity.Warehouse);
			}
			else 
			{
				OverNormModel.UpdateOperation(item, item.Param);
			}
		}
	}

	public class OverNormTempItem : PropertyChangedBase 
	{
		private OverNormItem item;
		public OverNormItem Item 
		{
			get => item;
			set => SetField(ref item, value);
		}

		private OverNormParam param;
		public OverNormParam Param 
		{
			get => param;
			set => SetField(ref param, value);
		}

		public OverNormTempItem(OverNormItem item, OverNormParam param) 
		{
			Item = item;
			Param = param;
		}
	}
}
