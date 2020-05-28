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

namespace workwear.Journal.ViewModels.Regulations
{
	public class ProfessionJournalViewModel : JournalViewModelBase
	{
		public ProfessionJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, ILifetimeScope autofacScope, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			Title = "Профессии";
			AutofacScope = autofacScope;
			var dataLoader = AutofacScope.Resolve<OracleSQLDataLoader<ProfessionJournalNode>>();
			DataLoader = dataLoader;
			dataLoader.AddQuery(MakeQuery, MapNode);
			CreateNodeActions();
		}

		#region Запрос

		void MakeQuery(OracleCommand cmd, bool isCounting, int? pageSize, int? skip)
		{
			string conditions = null; //" AND id_parent <> 0";

			conditions += OracleSQLDataLoader<ProfessionJournalNode>.MakeSearchConditions(Search.SearchValues,
				new[] {
					"ABBR",
				},
				new[] {
					"ID_REF"
				}
			);

			string sql;
			if(isCounting) {
				sql = $"select COUNT(*) from KIT.EXP_PROF";
				if(!String.IsNullOrEmpty(conditions))
					sql += " WHERE" + conditions.ReplaceFirstOccurrence(" AND", "");
			}
			else
				sql = $"SELECT t.* " +
					$"FROM (select rownum rnum, v.* from KIT.EXP_PROF v where rownum <= {skip + pageSize} {conditions}) t " +
					$"WHERE rnum > {skip ?? 0}";

			cmd.CommandText = sql;
		}

		ProfessionJournalNode MapNode(OracleDataReader reader)
		{
			return new ProfessionJournalNode() {
				Id = Convert.ToInt32(reader["ID_REF"]),
				Name = reader["ABBR"]?.ToString(),
			};
		}

		#endregion
	}

	public class ProfessionJournalNode
	{
		public int Id { get; set; }
		public uint? Code => (uint?)Id;
		public string Name { get; set; }
	}
}
