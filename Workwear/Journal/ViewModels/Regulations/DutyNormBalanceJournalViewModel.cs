using System;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Utilities;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using Workwear.Journal.Filter.ViewModels.Regulations;
using Workwear.Domain.Sizes;

namespace workwear.Journal.ViewModels.Regulations {
	public class DutyNormBalanceJournalViewModel: JournalViewModelBase {
		public DutyNormBalanceFilterViewModel Filter { get; set; }
		public DutyNormBalanceJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ILifetimeScope autofacScope,
			DutyNorm dutyNorm = null
		) : base(unitOfWorkFactory, interactiveService, navigationManager) {
			var dataLoader = new ThreadDataLoader<DutyNormBalanceJournalNode>(unitOfWorkFactory);
			dataLoader.AddQuery(ItemsQuery);
			DataLoader = dataLoader;
			JournalFilter =  Filter = autofacScope.Resolve<DutyNormBalanceFilterViewModel>(
				new TypedParameter(typeof(JournalViewModelBase), this),
				new TypedParameter(typeof(DutyNorm), dutyNorm)
			);
			Title = dutyNorm != null
				? $"Числится по дежурной норме {Filter.DutyNorm.Name}"
				: "Остатки по дежурным нормам";
			SelectionMode = JournalSelectionMode.Multiple;
		}

		#region Query

		public IQueryOver<DutyNormIssueOperation> ItemsQuery(IUnitOfWork unitOfWork) {
			DutyNormBalanceJournalNode resultAlias = null;
			DutyNormIssueOperation expenseOperationAlias = null;
			Nomenclature nomenclatureAlias = null;
			ItemsType nomenclatureItemTypesAlias = null;
			MeasurementUnits nomenclatureUnitsAlias = null;
			DutyNormIssueOperation removeOperationAlias = null;
			ProtectionTools protectionToolsAlias = null;
			ItemsType protectionToolsItemTypesAlias = null;
			MeasurementUnits protectionToolsUnitsAlias  = null;
			WarehouseOperation warehouseOperationAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			DutyNorm dutyNormAlias = null;

			var query = unitOfWork.Session.QueryOver(() => expenseOperationAlias);
			query.Where(GetSearchCriterion(
				() => dutyNormAlias.Name,
				() => nomenclatureAlias.Name
			));
			
			if(Filter.DutyNorm != null)
				query.Where(d=>d.DutyNorm == Filter.DutyNorm);
			if(Filter.Subdivision != null)
				query.Where(()=>dutyNormAlias.Subdivision.Id == Filter.Subdivision.Id);
			
			var subQueryRemove = QueryOver.Of(() => removeOperationAlias)
				.Where(() => removeOperationAlias.IssuedOperation.Id == expenseOperationAlias.Id)
				.Select(Projections.Sum<DutyNormIssueOperation>(o => o.Returned));
			
			var balance = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Int32,
				Projections.Property(() => expenseOperationAlias.Issued),
				Projections.SubQuery(subQueryRemove)
			);
			
			if(Filter.DutyNorm != null)
				query
					.JoinAlias(() => expenseOperationAlias.Nomenclature, () => nomenclatureAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => nomenclatureAlias.Type, () => nomenclatureItemTypesAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => nomenclatureItemTypesAlias.Units, () => nomenclatureUnitsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.ProtectionTools, () => protectionToolsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => protectionToolsAlias.Type, () => protectionToolsItemTypesAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => protectionToolsItemTypesAlias.Units, () => protectionToolsUnitsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.WarehouseOperation, () => warehouseOperationAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.DutyNorm, () => dutyNormAlias)
					.Where(Restrictions.Not(Restrictions.Eq(balance, 0)))
					.SelectList(list => list
						.SelectGroup(() => expenseOperationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
						.Select(() => nomenclatureUnitsAlias.Name).WithAlias(() => resultAlias.NomenclatureUnitsName)
						.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSize)
						.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
						.Select(() => warehouseOperationAlias.Cost).WithAlias(() => resultAlias.AvgCost)
						.Select(() => expenseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
						.Select(() => expenseOperationAlias.OperationTime).WithAlias(() => resultAlias.IssuedDate)
						.Select(() => expenseOperationAlias.StartOfUse).WithAlias(() => resultAlias.StartUseDate)
						.Select(() => expenseOperationAlias.ExpiryByNorm).WithAlias(() => resultAlias.ExpiryDate)
						.Select(() => expenseOperationAlias.AutoWriteoffDate).WithAlias(() => resultAlias.AutoWriteoffDate)
						.Select(() => dutyNormAlias.Name).WithAlias(() => resultAlias.DutyNormName)
						.Select(() => protectionToolsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsName)
						.Select(() => protectionToolsUnitsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsUnitsName)
						.Select(balance).WithAlias(() => resultAlias.Balance));
			else {
				query
					.JoinAlias(() => expenseOperationAlias.Nomenclature, () => nomenclatureAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.WearSize, () => sizeAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.Height, () => heightAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => nomenclatureAlias.Type, () => nomenclatureItemTypesAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => nomenclatureItemTypesAlias.Units, () => nomenclatureUnitsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.ProtectionTools, () => protectionToolsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => protectionToolsAlias.Type, () => protectionToolsItemTypesAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => protectionToolsItemTypesAlias.Units, () => protectionToolsUnitsAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.WarehouseOperation, () => warehouseOperationAlias, JoinType.LeftOuterJoin)
					.JoinAlias(() => expenseOperationAlias.DutyNorm, () => dutyNormAlias)
					.Where(Restrictions.Not(Restrictions.Eq(balance, 0)))
					.SelectList(list => list
						.Select(() => expenseOperationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
						.Select(() => nomenclatureUnitsAlias.Name).WithAlias(() => resultAlias.NomenclatureUnitsName)
						.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.WearSize)
						.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
						.Select(() => warehouseOperationAlias.Cost).WithAlias(() => resultAlias.AvgCost)
						.Select(() => expenseOperationAlias.WearPercent).WithAlias(() => resultAlias.WearPercent)
						.Select(() => expenseOperationAlias.OperationTime).WithAlias(() => resultAlias.IssuedDate)
						.Select(() => expenseOperationAlias.StartOfUse).WithAlias(() => resultAlias.StartUseDate)
						.Select(() => expenseOperationAlias.ExpiryByNorm).WithAlias(() => resultAlias.ExpiryDate)
						.Select(() => expenseOperationAlias.AutoWriteoffDate).WithAlias(() => resultAlias.AutoWriteoffDate)
						.Select(() => dutyNormAlias.Name).WithAlias(() => resultAlias.DutyNormName)
						.Select(() => protectionToolsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsName)
						.Select(() => protectionToolsUnitsAlias.Name).WithAlias(() => resultAlias.ProtectionToolsUnitsName)
						.Select(balance).WithAlias(() => resultAlias.Balance));
				query = query.OrderBy(()=>dutyNormAlias.Name).Asc;
			}
			return query.TransformUsing(Transformers.AliasToBean<DutyNormBalanceJournalNode>());
		}

		#endregion
	}

	public class DutyNormBalanceJournalNode 
	{
		public int Id { get; set; }
		public string NomenclatureName { get; set; }
		public string ProtectionToolsName { get; set; }
		public string ItemName => NomenclatureName ?? ProtectionToolsName;
		public string NomenclatureUnitsName { get; set;}
		public string ProtectionToolsUnitsName { get; set;}
		public string UnitsName => NomenclatureUnitsName ?? ProtectionToolsUnitsName;
		public string WearSize { get; set; }
		public string Height { get; set; }
		public decimal AvgCost { get; set; }
		public decimal WearPercent { get; set; }
		public DateTime IssuedDate { get; set; }
		public DateTime? StartUseDate { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public DateTime? AutoWriteoffDate { get; set; }
		public decimal Percentage => ExpiryDate != null ? DutyNormIssueOperation.CalculatePercentWear(DateTime.Today, StartUseDate, ExpiryDate, WearPercent) : 0;
		public int Balance { get; set; }
		public string BalanceText => $"{Balance} {UnitsName}";
		public string AvgCostText => AvgCost > 0 ? CurrencyWorks.GetShortCurrencyString (AvgCost) : String.Empty;
		public string DutyNormName {get;set;}
	}
}
