using System;
using System.Linq;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Utilities;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;

namespace Workwear.ViewModels.Company.EmployeeChildren {
	public class EmployeeOverNormViewModel : ViewModelBase{
		
		private readonly INavigationManager navigation;
		private readonly EmployeeViewModel employeeViewModel;
		
		public EmployeeCard Entity => employeeViewModel.Entity;
		private IUnitOfWork UoW => employeeViewModel.UoW;
		private bool isConfigured = false;
		
		public EmployeeOverNormViewModel(INavigationManager navigation, EmployeeViewModel employeeViewModel) {
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
		}

		public void LoadNodes() {
			EmployeeOverNormNode resultAlias = null;
			OverNormOperation overNormOperationAlias = null;
			OverNormOperation removeOperationAlias = null;
			OverNormItem overNormDocItemAlias = null;
			OverNorm overNormDocAlias = null;
			WarehouseOperation warehouseOperationAlias = null;
WarehouseOperation warehouseOperationAlias1 = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType nomenclatureItemTypesAlias = null;
			MeasurementUnits nomenclatureUnitsAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			
			
			var query = UoW.Session.QueryOver(() => overNormOperationAlias)
				.Where(e => e.Employee.Id == Entity.Id);
/*
			var subQueryRemove = QueryOver.Of(() => removeOperationAlias)
				.JoinAlias(() => overNormOperationAlias.WarehouseOperation, () => warehouseOperationAlias1, JoinType.LeftOuterJoin)
				.Where(() => removeOperationAlias.WriteOffOverNormOperation.Id == overNormOperationAlias.Id)
				.Select (Projections.Sum<WarehouseOperation> (o => o.Amount));
*/
			query
				.JoinEntityAlias(() => overNormDocItemAlias, () => overNormDocItemAlias.OverNormOperation.Id == overNormOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => overNormDocItemAlias.Document, () => overNormDocAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => overNormOperationAlias.WarehouseOperation, () => warehouseOperationAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => warehouseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => warehouseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => nomenclatureAlias.Type, () => nomenclatureItemTypesAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => nomenclatureItemTypesAlias.Units, () => nomenclatureUnitsAlias, JoinType.LeftOuterJoin)
//.Where (e => e.AutoWriteoffDate == null || e.AutoWriteoffDate > DateTime.Today)
				.SelectList (list => list
//.SelectGroup (() => expenseOperationAlias.Id).WithAlias (() => resultAlias.Id)
					.Select(() => overNormOperationAlias.Id).WithAlias (() => resultAlias.Id)
					.Select(() => overNormDocAlias.Type).WithAlias (() => resultAlias.DocType)
					.Select(() => nomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.Select(() => nomenclatureUnitsAlias.Name).WithAlias (() => resultAlias.NomenclatureUnitsName)
					.Select(() => sizeAlias.Name).WithAlias (() => resultAlias.WearSize)
					.Select(() => heightAlias.Name).WithAlias (() => resultAlias.Height)
					.Select(() => warehouseOperationAlias.Cost).WithAlias (() => resultAlias.AvgCost)
					.Select(() => warehouseOperationAlias.WearPercent).WithAlias (() => resultAlias.WearPercent)
					.Select(() => warehouseOperationAlias.Amount).WithAlias (() => resultAlias.Added)
					.Select(() => warehouseOperationAlias.OperationTime).WithAlias (() => resultAlias.Date)
					//.SelectSubQuery (subQueryRemove).WithAlias (() => resultAlias.Removed)
				);
			
			var items = query
					.TransformUsing (Transformers.AliasToBean<EmployeeOverNormNode>())
					.List<EmployeeOverNormNode>()
					.Where(r => r.Added - r.Removed != 0);
			foreach(var item in items) 
				ObservableItems.Add(item);
		}
				
		public void OnShow() {
			if(!isConfigured) {
				isConfigured = true;
				LoadNodes();
				OnPropertyChanged(nameof(ObservableItems));
			}
		}

		#region  Свойства View
		public IObservableList<EmployeeOverNormNode> ObservableItems { get; set; } = new ObservableList<EmployeeOverNormNode>();
		#endregion

		#region Методы
		
		public void AddItem() {
		}

		public void DeleteItem() {
		}
		#endregion
	}
	
	public class EmployeeOverNormNode {
		public int Id { get; set; }
		public OverNormType DocType   { get; set;}
		public string NomenclatureName { get; set;}
		public string NomenclatureUnitsName { get; set;}
		public string WearSize { get; set; }
		public string Height { get; set; }
		public decimal AvgCost { get; set;}
		public decimal WearPercent { get; set;}
		public string WearPercentString => (WearPercent * 0.01m).ToString("P0"); 
		public DateTime Date { get; set;}
		public string DateString => Date.ToShortDateString();
		public int Added { get; set;}
		public int Removed { get; set; } = 0;
		public int Balance => Added - Removed;
		public string BalanceText => $"{Balance} {NomenclatureUnitsName}";
		public string AvgCostText => AvgCost > 0 ? CurrencyWorks.GetShortCurrencyString(AvgCost) : String.Empty;
	}
}
