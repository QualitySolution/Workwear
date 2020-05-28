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
	public class PostJournalViewModel : JournalViewModelBase
	{
		public PostJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, ILifetimeScope autofacScope, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			Title = "Штатное расписание";
			AutofacScope = autofacScope;
			var dataLoader = AutofacScope.Resolve<OracleSQLDataLoader<PostJournalNode>>();
			DataLoader = dataLoader;
			dataLoader.AddQuery(MakeQuery, MapNode);
			CreateNodeActions();
		}

		#region Запрос

		void MakeQuery(OracleCommand cmd, bool isCounting, int? pageSize, int? skip)
		{
			string conditions = null; 

			conditions += OracleSQLDataLoader<PostJournalNode>.MakeSearchConditions(Search.SearchValues,
				new[] {
					"WP_NAME",
				},
				new[] {
					"ID_WP",
					"DEPT_CODE",
				}
			);

			string sql;
			if(isCounting) {
				sql = $"select COUNT(*) from KIT.WP";
				if(!String.IsNullOrEmpty(conditions))
					sql += " WHERE" + conditions.ReplaceFirstOccurrence(" AND", "");
			}
			else
				sql = $"SELECT t.*, dep.DEPT as department, prof.ABBR as profession, sub.NGRPOL as subdivision " +
					$"FROM (select rownum rnum, v.* from KIT.WP v where rownum <= {skip + pageSize} {conditions}) t " +
					"LEFT JOIN KIT.EXP_PROF prof ON prof.ID_REF = t.E_PROF " +
					"LEFT JOIN KIT.DEPTS dep ON dep.ID_DEPT = t.ID_DEPT " +
					"LEFT JOIN SKLAD.SGRPOL sub ON sub.KGRPOL = dep.DEPT_CODE " +
					$"WHERE rnum > {skip ?? 0}";

			cmd.CommandText = sql;
		}

		PostJournalNode MapNode(OracleDataReader reader)
		{
			return new PostJournalNode() {
				Id = Convert.ToInt32(reader["ID_WP"]),
				Name = reader["WP_NAME"]?.ToString(),
				Department = reader["department"]?.ToString(),
				Profession = reader["profession"]?.ToString(),
				Subdivision = reader["subdivision"]?.ToString(),
			};
		}

		#endregion
	}

	public class PostJournalNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Subdivision { get; set; }
		public string Department { get; set; }
		public string Profession { get; set; }
	}
}
