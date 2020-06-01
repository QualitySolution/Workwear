using System;
using Autofac;
using NHibernate;
using NHibernate.Transform;
using Oracle.ManagedDataAccess.Client;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using workwear.Domain.Company;
using workwear.Tools.Oracle;
using workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{
	public class SubdivisionJournalViewModel : JournalViewModelBase
	{
		public SubdivisionJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, ILifetimeScope autofacScope) : base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			Title = "Подразделения(Цеха)";
			AutofacScope = autofacScope;
			var dataLoader = AutofacScope.Resolve<OracleSQLDataLoader<SubdivisionJournalNode>>();
			DataLoader = dataLoader;
			dataLoader.AddQuery(MakeQuery, MapNode);
			CreateNodeActions();
		}

		#region Запрос

		void MakeQuery(OracleCommand cmd, bool isCounting, int? pageSize, int? skip)
		{
			string conditions = null;

			conditions += OracleSQLDataLoader<EmployeeJournalNode>.MakeSearchConditions(Search.SearchValues,
				new[] {
					"KGRPOL",
					"NGRPOL",
				},
				null
			);

			string sql;
			if(isCounting)
				sql = $"select COUNT(*) from SKLAD.SGRPOL";
			else
				sql = $"SELECT * from SKLAD.SGRPOL";


			if(!String.IsNullOrEmpty(conditions))
				sql += " WHERE" + conditions.ReplaceFirstOccurrence(" AND", "");

			cmd.CommandText = sql;
		}

		SubdivisionJournalNode MapNode(OracleDataReader reader)
		{
			return new SubdivisionJournalNode() {
				Id = Convert.ToInt32(reader["KGRPOL"]),
				Code = reader["KGRPOL"]?.ToString(),
				Name = reader["NGRPOL"]?.ToString(),
			};
		}

		#endregion
	}

	public class SubdivisionJournalNode
	{
		public int Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
	}
}
