using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Operations;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock.Documents;
using Workwear.Models.Operations;

namespace Workwear.Repository.Operations
{
	public class EmployeeIssueRepository
	{
		private readonly UnitOfWorkProvider unitOfWorkProvider;
		private IUnitOfWork repoUow;
		
		[Obsolete("Лучше используйте конструктор с провайдером Uow")]
		public EmployeeIssueRepository(IUnitOfWork uow)
		{
			RepoUow = uow;
		}
		
		public EmployeeIssueRepository(UnitOfWorkProvider unitOfWorkProvider = null) {
			this.unitOfWorkProvider = unitOfWorkProvider;
		}

		public IUnitOfWork RepoUow {
			get => repoUow ?? unitOfWorkProvider.UoW;
			set => repoUow = value;
		}

		/// <summary>
		/// Получаем все операции выдачи сотруднику отсортированные в порядке убывания.
		/// </summary>
		/// <returns></returns>
		public virtual IList<EmployeeIssueOperation> AllOperationsForEmployee(
			EmployeeCard employee, 
			Action<IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null, IUnitOfWork uow = null)
		{
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee == employee);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Desc.List();
		}

		/// <summary>
		/// Получаем все операции выдачи сотрудникам.
		/// </summary>
		/// <returns></returns>
		public virtual IList<EmployeeIssueOperation> AllOperationsFor(
			EmployeeCard[] employees = null,
			ProtectionTools[] protectionTools = null,
			Action<IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null,
			IUnitOfWork uow = null)
		{
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>();
			if(employees != null && employees.Any())
				query.Where(o => o.Employee.IsIn(employees));

			if(protectionTools != null && protectionTools.Any())
				query.Where(o => o.ProtectionTools.IsIn(protectionTools));

			makeEager?.Invoke(query);

			return query.List();
		}

		/// <summary>
		/// Получаем операции выдачи выполненные в определенные даты.
		/// </summary>
		public IList<EmployeeIssueOperation> GetOperationsByDates(
			EmployeeCard[] employees, 
			DateTime begin, DateTime end, 
			Action<IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null)
		{
			var employeeIds = employees.Select(x => x.Id).Distinct().ToArray();
			var query = RepoUow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee.Id.IsIn(employeeIds))
				//Проверяем попадает ли операция в диапазон, обратным сравнением условий.
				//Проверяем 2 даты и начала и конца,
				//так как по сути для наса важны StartOfUse и ExpiryByNorm но они могут быть null.
				.Where(o => (o.OperationTime < end.Date.AddDays(1)) && (o.OperationTime >= begin));
			makeEager?.Invoke(query);
			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		/// <summary>
		/// Получаем операции числящееся по сотрудникам которых затрагивает определенный диапазон дат.
		/// </summary>
		public virtual IList<EmployeeIssueOperation> GetOperationsTouchDates(IUnitOfWork uow, EmployeeCard[] employees, DateTime begin, DateTime end, Action<NHibernate.IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null) { 

			var employeeIds = employees.Select(x => x.Id).Distinct().ToArray();

			var query = uow.Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee.Id.IsIn(employeeIds))
				//Проверяем попадает ли операция в диапазон, обратным сравнением условий.
				//Проверяем 2 даты и начала и конца,
				//так как по сути для наса важны StartOfUse и ExpiryByNorm но они могут быть null.
				.Where(o => (o.OperationTime <= end || o.StartOfUse <= end) 
				            && (o.ExpiryByNorm >= begin || o.AutoWriteoffDate >= begin));

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		public IList<EmployeeIssueOperation> GetOperationsForEmployee(
			EmployeeCard employee, 
			ProtectionTools protectionTools, 
			IUnitOfWork uow = null,
			Action<IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null)
		{
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee == employee)
				.Where(o => o.ProtectionTools == protectionTools);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}

		public IList<EmployeeIssueOperation> GetLastIssueOperationsForEmployee(IEnumerable<EmployeeCard> employees) =>
			GetLastIssueOperationsForEmployee(employees,null,null);
		public IList<EmployeeIssueOperation> GetLastIssueOperationsForEmployee(
			IEnumerable<EmployeeCard> employees, 
			Action<IQueryOver<EmployeeIssueOperation, 
				EmployeeIssueOperation>> makeEager = null, IUnitOfWork uow = null)
		{
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			EmployeeIssueOperation employeeIssueOperation2Alias = null;
			var ids = employees.Select(x => x.Id).Distinct().ToArray();
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>(() => employeeIssueOperationAlias)
				.Where(o => o.Employee.IsIn(ids))
				.Where(() => employeeIssueOperationAlias.Issued > 0)
				.JoinEntityAlias(() => employeeIssueOperation2Alias,
					() => employeeIssueOperationAlias.ProtectionTools.Id == employeeIssueOperation2Alias.ProtectionTools.Id
					      && employeeIssueOperationAlias.Employee.Id == employeeIssueOperation2Alias.Employee.Id
					      && employeeIssueOperation2Alias.Issued > 0
					      && employeeIssueOperationAlias.OperationTime < employeeIssueOperation2Alias.OperationTime
					, JoinType.LeftOuterJoin)
				.Where(() => employeeIssueOperation2Alias.Id == null);

			makeEager?.Invoke(query);

			return query.List();
		}

		public IList<EmployeeIssueOperation> GetLast2IssueOperationsForEmployee(
			IEnumerable<EmployeeCard> employees) => GetLast2IssueOperationsForEmployee(employees,null);
		public IList<EmployeeIssueOperation> GetLast2IssueOperationsForEmployee(
			IEnumerable<EmployeeCard> employees, IUnitOfWork uow = null) {
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			var ids = employees.Select(x => x.Id).Distinct().ToArray();

			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>(() => employeeIssueOperationAlias)
				.Where(o => o.Employee.IsIn(ids))
				.Where(() => employeeIssueOperationAlias.Issued > 0)
				.OrderBy(o => o.Employee.Id).Asc
				.ThenBy(o => o.ProtectionTools.Id).Asc
				.ThenBy(o => o.OperationTime).Desc
				.List();

			IList<EmployeeIssueOperation> result = new List<EmployeeIssueOperation>();
			foreach(var employeeGroup in query.GroupBy(x => x.Employee)) 
				foreach(var protectionToolsGroup in employeeGroup.GroupBy(x => x.ProtectionTools)) 
					foreach(var op in protectionToolsGroup.Take(2)) 
						result.Add(op);
			return result;
		}

		/// <summary>
		/// Получаем все операции выдачи выданные по указанной строке нормы.
		/// </summary>
		/// <returns></returns>
		public IList<EmployeeIssueOperation> GetOperationsForNormItem(
			NormItem[] normItems, Action<IQueryOver<EmployeeIssueOperation, 
				EmployeeIssueOperation>> makeEager = null, IUnitOfWork uow = null, DateTime? beginDate = null) {
			var itemIds = normItems.Select(i => i.Id).ToArray();
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.NormItem.Id.IsIn(itemIds));
			if(beginDate.HasValue)
				query.Where(o => o.OperationTime >= beginDate);
			makeEager?.Invoke(query);
			return query.List();
		}
		public IList<OperationToDocumentReference> GetReferencedDocuments(params int[] operationsIds)
		{
			OperationToDocumentReference docAlias = null;
			EmployeeIssueOperation employeeIssueOperationAlias = null;
			Expense expenseAlias = null;
			ExpenseItem expenseItemAlias = null;
			Return returnAlias = null;
			ReturnItem returnItemAlias = null;
			CollectiveExpense collectiveExpenseAlias = null;
			CollectiveExpenseItem collectiveExpenseItemAlias = null;
			Writeoff writeoffAlias = null;
			WriteoffItem writeoffItemAlias = null;
			Inspection inspectionAlias = null;
			InspectionItem inspectionItemAlias = null;
			
			var result = RepoUow.Session.QueryOver<EmployeeIssueOperation>(() => employeeIssueOperationAlias)
				.JoinEntityAlias(() => expenseItemAlias, () => expenseItemAlias.EmployeeIssueOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => expenseItemAlias.ExpenseDoc, () => expenseAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => collectiveExpenseItemAlias, () => collectiveExpenseItemAlias.EmployeeIssueOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => collectiveExpenseItemAlias.Document, () => collectiveExpenseAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => returnItemAlias, () => returnItemAlias.ReturnFromEmployeeOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => returnItemAlias.Document, () => returnAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => writeoffItemAlias, () => writeoffItemAlias.EmployeeWriteoffOperation.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => writeoffItemAlias.Document, () => writeoffAlias, JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => inspectionItemAlias, () => inspectionItemAlias.NewOperationIssue.Id == employeeIssueOperationAlias.Id, JoinType.LeftOuterJoin)
				.JoinAlias(() => inspectionItemAlias.Document, () => inspectionAlias, JoinType.LeftOuterJoin)
				.Where(x => x.Id.IsIn(operationsIds))
				.SelectList(list => list
					.Select(i => i.Id).WithAlias(() => docAlias.OperationId)
					.Select(() => expenseItemAlias.Id).WithAlias(() => docAlias.ExpenseItemId)
					.Select(() => expenseItemAlias.ExpenseDoc.Id).WithAlias(() => docAlias.ExpenseId)
					.Select(() => expenseAlias.DocNumber).WithAlias(() => docAlias.ExpenseDocNumber)
					.Select(() => collectiveExpenseItemAlias.Id).WithAlias(() => docAlias.CollectiveExpenseItemId)
					.Select(() => collectiveExpenseItemAlias.Document.Id).WithAlias(() => docAlias.CollectiveExpenseId)
					.Select(() => collectiveExpenseAlias.DocNumber).WithAlias(() => docAlias.CollectiveExpenseDocNumber)
					.Select(() => returnItemAlias.Id).WithAlias(() => docAlias.ReturnItemId)
					.Select(() => returnItemAlias.Document.Id).WithAlias(() => docAlias.ReturnId)
					.Select(() => returnAlias.DocNumber).WithAlias(() => docAlias.ReturnDocNumber)
					.Select(() => writeoffItemAlias.Id).WithAlias(() => docAlias.WriteoffItemId)
					.Select(() => writeoffItemAlias.Document.Id).WithAlias(() => docAlias.WriteoffId)
					.Select(() => writeoffAlias.DocNumber).WithAlias(() => docAlias.WriteoffDocNumber)
					.Select(() => inspectionItemAlias.Id).WithAlias(() => docAlias.InspectionItemId)
					.Select(() => inspectionItemAlias.Document.Id).WithAlias(() => docAlias.InspectionId)
					.Select(() => inspectionAlias.DocNumber).WithAlias(() => docAlias.InspectionDocNumber)
				)
				.TransformUsing(Transformers.AliasToBean<OperationToDocumentReference>())
				.List<OperationToDocumentReference>();

			return result;
		}

		public IList<EmployeeIssueOperation> GetAllManualIssue(
			IUnitOfWork uoW, 
			EmployeeCard employee, 
			ProtectionTools protectionTools, 
			Action<IQueryOver<EmployeeIssueOperation, EmployeeIssueOperation>> makeEager = null) {
			var query = (uoW ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.Employee == employee)
				.Where(o => o.ProtectionTools == protectionTools)
				.Where(o => o.ManualOperation == true);

			makeEager?.Invoke(query);

			return query.OrderBy(x => x.OperationTime).Asc.List();
		}
		
		/// <summary>
		/// Метод подсчитывает количество числящегося за сотрудником на определенную дату.
		/// ВНИМАНИЕ!! Метод пока не учитывает ручные операции скидывающие историю. Это надо будет дорабатывать.
		/// </summary>
		public virtual IList<EmployeeReceivedInfo> ItemsBalance(EmployeeCard employee, DateTime onDate, int[] excludeOperationsIds = null, IUnitOfWork uow = null)
		{
			EmployeeReceivedInfo resultAlias = null;

			EmployeeIssueOperation employeeIssueOperationAlias = null;
			EmployeeIssueOperation employeeIssueOperationReceivedAlias = null;

			if(excludeOperationsIds == null)
				excludeOperationsIds = new int[] { };

			IProjection projection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Int32, "SUM(IFNULL(?1, 0) - IFNULL(?2, 0))"),
				NHibernateUtil.Int32,
				Projections.Property<EmployeeIssueOperation>(x => x.Issued),
				Projections.Property<EmployeeIssueOperation>(x => x.Returned)
			);

			IProjection projectionIssueDate = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Date, "MAX(CASE WHEN ?1 > 0 THEN ?2 END)"),
				NHibernateUtil.Date,
				Projections.Property<EmployeeIssueOperation>(x => x.Issued),
				Projections.Property<EmployeeIssueOperation>(x => x.OperationTime)
			);

			return (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>(() => employeeIssueOperationAlias)
				.Left.JoinAlias(x => x.IssuedOperation, () => employeeIssueOperationReceivedAlias)
				.Where(x => x.Employee == employee)
				.Where(() => employeeIssueOperationAlias.OperationTime < onDate.AddDays(1))
				.WhereNot(() => employeeIssueOperationAlias.Id.IsIn(excludeOperationsIds))
				.Where(Restrictions.Or(
					Restrictions.Conjunction()
						.Add(Restrictions.Where( () => employeeIssueOperationAlias.Issued > 0))
						.Add(Restrictions.Where( () => employeeIssueOperationAlias.AutoWriteoffDate == null || employeeIssueOperationAlias.AutoWriteoffDate > onDate)),
					Restrictions.Conjunction()
						.Add(Restrictions.Where(() => employeeIssueOperationAlias.Returned > 0))
						.Add(Restrictions.Where(() => employeeIssueOperationReceivedAlias.AutoWriteoffDate == null || employeeIssueOperationReceivedAlias.AutoWriteoffDate > onDate))
					))
				.SelectList(list => list
				   .SelectGroup(() => employeeIssueOperationAlias.ProtectionTools.Id).WithAlias(() => resultAlias.ProtectionToolsId)
				   .SelectGroup(() => employeeIssueOperationAlias.NormItem.Id).WithAlias(() => resultAlias.NormRowId)
				   .Select(projectionIssueDate).WithAlias(() => resultAlias.LastReceive)
				   .Select(projection).WithAlias(() => resultAlias.Amount)
				   .Select(() => employeeIssueOperationAlias.Nomenclature.Id).WithAlias(()=> resultAlias.NomenclatureId)
				)
				.TransformUsing(Transformers.AliasToBean<EmployeeReceivedInfo>())
				.List<EmployeeReceivedInfo>();
		}
		
		/// <summary>
		/// Получаем все операции выдачи указанному сотруднику по определенной строке нормы.
		/// Исключаем ручные операции.
		/// </summary>
		/// <returns></returns>
		public IList<EmployeeIssueOperation> GetIssueOperationsForEmployeeWithNormItems(int employeeId, int[] normItemsIds, IUnitOfWork uow = null) {
			var query = (uow ?? RepoUow).Session.QueryOver<EmployeeIssueOperation>()
				.Where(o => o.NormItem.Id.IsIn(normItemsIds))
				.Where(o => o.Employee.Id == employeeId)
				.Where(o => o.Issued > 0)
				.Where(o => o.ManualOperation == false)
				.List();
			if (query.Any(o => o.WarehouseOperation == null))
				throw new InvalidOperationException("Складская операция должна быть заполнена.");
			return query;
		}
	}
	
	public class EmployeeReceivedInfo
	{
		public int? NormRowId { get; set; }

		public int? ProtectionToolsId { get; set;}
		
		public int? NomenclatureId { get; set; }

		public DateTime LastReceive { get; set;}

		public int Amount { get; set;}
	}
}
