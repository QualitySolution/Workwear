using System;
using Autofac;
using Oracle.ManagedDataAccess.Client;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using workwear.Tools.Oracle;

namespace workwear.Journal.ViewModels.Company
{

	public class DepartmentJournalViewModel : JournalViewModelBase
	{
		public DepartmentJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, ILifetimeScope autofacScope, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			Title = "Отделы";
			AutofacScope = autofacScope;
			var dataLoader = AutofacScope.Resolve<OracleSQLDataLoader<DepartmentJournalNode>>();
			DataLoader = dataLoader;
			dataLoader.AddQuery(MakeQuery, MapNode);
			CreateNodeActions();
		}

		#region Запрос

		void MakeQuery(OracleCommand cmd, bool isCounting, int? pageSize, int? skip)
		{
			string conditions = null; //" AND id_parent <> 0";

			conditions += OracleSQLDataLoader<DepartmentJournalNode>.MakeSearchConditions(Search.SearchValues,
				new[] {
					"DEPT",
				},
				new[] {
					"DEPT_CODE"
				}
			);

			string sql;
			if(isCounting) {
				sql = $"select COUNT(*) from KIT.DEPTS";
				if(!String.IsNullOrEmpty(conditions))
					sql += " WHERE" + conditions.ReplaceFirstOccurrence(" AND", "");
			}
			else
				sql = $"SELECT t.*, sub.NGRPOL as subdivision " +
					$"FROM (select rownum rnum, v.* from KIT.DEPTS v where rownum <= {skip + pageSize} {conditions}) t " +
					"LEFT JOIN SKLAD.SGRPOL sub ON sub.KGRPOL = t.DEPT_CODE " +
					//						"LEFT JOIN KIT.DEPTS sub ON sub.DEPT_CODE = t.DEPT_CODE AND sub.ID_PARENT = 0 " +
					$"WHERE rnum > {skip ?? 0}";

			cmd.CommandText = sql;
		}

		DepartmentJournalNode MapNode(OracleDataReader reader)
		{
			return new DepartmentJournalNode() {
				Id = Convert.ToInt32(reader["ID_DEPT"]),
				Subdivision = reader["subdivision"]?.ToString(),
				Name = reader["DEPT"]?.ToString(),
				SubdivisionCode = reader["DEPT_CODE"]?.ToString(),
			};
		}

		#endregion
	}

	public class DepartmentJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string SubdivisionCode { get; set; }
		public string Subdivision { get; set; }
		public string Comments { get; set; }
	}
}
