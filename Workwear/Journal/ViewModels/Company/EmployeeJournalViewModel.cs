using System;
using Autofac;
using Gamma.ColumnConfig;
using Oracle.ManagedDataAccess.Client;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.Utilities.Text;
using workwear.Journal.Filter.ViewModels.Company;
using workwear.Tools.Oracle;

namespace workwear.Journal.ViewModels.Company
{
	public class EmployeeJournalViewModel : JournalViewModelBase
	{
		private readonly ITdiCompatibilityNavigation tdiNavigationManager;

		/// <summary>
		/// Для хранения пользовательской информации как в WinForms
		/// </summary>
		public object Tag;

		public EmployeeFilterViewModel Filter { get; private set; }

		public EmployeeJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, ITdiCompatibilityNavigation navigationManager, 
										ILifetimeScope autofacScope, ICurrentPermissionService currentPermissionService = null) 
										: base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			UseSlider = false;
			Title = "Сотрудники";
			AutofacScope = autofacScope;
			var dataLoader = AutofacScope.Resolve<OracleSQLDataLoader<EmployeeJournalNode>>();
			DataLoader = dataLoader;
			dataLoader.AddQuery(MakeQuery, MapNode);

			JournalFilter = Filter = AutofacScope.Resolve<EmployeeFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
			this.tdiNavigationManager = navigationManager;
		}

		//protected override void EditEntityDialog(EmployeeJournalNode node)
		//{
		//	tdiNavigationManager.OpenTdiTab<EmployeeCardDlg, int>(this, node.Id);
		//}

		#region Запрос

		void MakeQuery(OracleCommand cmd, bool isCounting, int? pageSize, int? skip)
		{
			string conditions = null;
			if(Filter.ShowOnlyWork)
				conditions += " AND DUVOL IS NULL";

			conditions += OracleSQLDataLoader<EmployeeJournalNode>.MakeSearchConditions(Search.SearchValues,
				new[] {
					"SURNAME",
					"NAME",
					"SECNAME"
				},
				new[] {
					"TN"
				}

			);

			string sql;
			if(isCounting) {
				sql = $"select COUNT(*) from v_sklad_spec_human_l_full";
				if(conditions != null)
					sql += " WHERE" + conditions.ReplaceFirstOccurrence(" AND", "");
			}
			else //Дурацкий оракл, сервер используется версии 11. Там нет аналога Limit, поэтому такой сложный запрос.S
				sql = $"SELECT t.*, dept.NGRPOL dept_name, prof.ABBR prof_name " +
					$"FROM (select rownum rnum, v.* from v_sklad_spec_human_l_full v where rownum <= {skip + pageSize} {conditions}) t " +
					"LEFT JOIN SGRPOL dept ON dept.KGRPOL = t.DEPT_CODE " +
					"LEFT JOIN KIT.EXP_PROF prof ON prof.ID_REF = t.E_PROF " +
					$"WHERE rnum > {skip ?? 0}";
	
			cmd.CommandText = sql;

		}

		EmployeeJournalNode MapNode(OracleDataReader reader)
		{
			return new EmployeeJournalNode() {
				CardNumber = String.Empty,
				FirstName = reader["NAME"]?.ToString(),
				LastName = reader["SURNAME"]?.ToString(),
				Patronymic = reader["SECNAME"]?.ToString(),
				PersonnelNumber = reader["TN"]?.ToString(),
				DismissDate = reader["DUVOL"] as DateTime?,
				Subdivision = reader["dept_name"]?.ToString(),
				Post = reader["prof_name"]?.ToString()
			};
		}

		#endregion
	}

	public class EmployeeJournalNode
	{
		public int Id { get; set; }
		[SearchHighlight]
		public string CardNumber { get; set; }

		[SearchHighlight]
		public string CardNumberText {
			get {
				return CardNumber ?? Id.ToString();
			}
		}

		[SearchHighlight]
		public string PersonnelNumber { get; set; }

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Patronymic { get; set; }

		[SearchHighlight]
		public string FIO {
			get {
				return String.Join(" ", LastName, FirstName, Patronymic);
			}
		}

		public string Post { get; set; }

		public string Subdivision { get; set; }

		public bool Dismiss { get { return DismissDate.HasValue; } }

		public DateTime? DismissDate { get; set; }
		public string Title => PersonHelper.PersonNameWithInitials(LastName, FirstName, Patronymic);
	}
}
