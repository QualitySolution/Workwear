using System;
using System.Linq;
using Autofac;
using Gamma.ColumnConfig;
using Oracle.ManagedDataAccess.Client;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using workwear.Dialogs.Organization;
using workwear.Domain.Company;
using workwear.Journal.Filter.ViewModels.Company;
using workwear.Repository.Company;
using workwear.Tools.Oracle;

namespace workwear.Journal.ViewModels.Company
{
	public class EmployeeJournalViewModel : JournalViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly ITdiCompatibilityNavigation tdiNavigationManager;
		private readonly HRSystem hrSystem;
		private readonly IDeleteEntityService deleteEntityService;

		/// <summary>
		/// Для хранения пользовательской информации как в WinForms
		/// </summary>
		public object Tag;

		public EmployeeFilterViewModel Filter { get; private set; }

		public EmployeeJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, ITdiCompatibilityNavigation navigationManager, 
										ILifetimeScope autofacScope, HRSystem hrSystem, IDeleteEntityService deleteEntityService, ICurrentPermissionService currentPermissionService = null) 
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
			this.hrSystem = hrSystem ?? throw new ArgumentNullException(nameof(hrSystem));
			this.deleteEntityService = deleteEntityService ?? throw new ArgumentNullException(nameof(deleteEntityService));
			CreateNodeActions();
		}

		#region Действия

		protected override void CreateNodeActions()
		{
			base.CreateNodeActions();

			var addAction = new JournalAction("Добавить",
					(selected) => true,
					(selected) => true,
					(selected) => CreateEntityDialog()
					);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => selected.Cast<EmployeeJournalNode>().ToList().ForEach(EditEntityDialog)
					);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
				RowActivatedAction = editAction;

			var deleteAction = new JournalAction("Удалить",
					(selected) => selected.Cast<EmployeeJournalNode>().Any(x => x.Id.HasValue && String.IsNullOrEmpty(x.PersonnelNumber)),
					(selected) => true,
					(selected) => DeleteEntities(selected.Cast<EmployeeJournalNode>().Where(x => x.Id.HasValue && String.IsNullOrEmpty(x.PersonnelNumber)).ToArray())
					);
			NodeActionsList.Add(deleteAction);
		}

		protected void EditEntityDialog(EmployeeJournalNode node)
		{
			int cardId;
			if(node.Id.HasValue)
				cardId = node.Id.Value;
			else {
				var card = EmployeeRepository.GetEmployeeByPersonalNumber(UoW, node.PersonnelNumber);
				if(card == null) {
					logger.Info("Карточка сотрудника не найдена. Создаем новую.");
					card = new Domain.Company.EmployeeCard() {
						PersonnelNumber = node.PersonnelNumber,
						LastName = node.LastName,
						FirstName = node.FirstName,
						Patronymic = node.Patronymic,
						DismissDate = node.DismissDate,
						Sex = node.Sex

						//FIXME добавить заполнение подразделение и должности.
					};
					UoW.Save(card);
					UoW.Commit();
				}
				cardId = card.Id;
			}
			tdiNavigationManager.OpenTdiTab<EmployeeCardDlg, int>(this, cardId);
		}

		protected virtual void CreateEntityDialog()
		{
			tdiNavigationManager.OpenTdiTab<EmployeeCardDlg>(this);
		}

		protected virtual void DeleteEntities(EmployeeJournalNode[] nodes)
		{
			foreach(var node in nodes)
				deleteEntityService.DeleteEntity<EmployeeCard>(node.Id.Value);
		}

		#endregion

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
				Post = reader["prof_name"]?.ToString(),
				NSex = reader["E_SEX"] as decimal?
			};
		}

		#endregion
	}

	public class EmployeeJournalNode
	{
		public int? Id { get; set; }
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

		public decimal? NSex { get; set; }

		public Sex Sex => NSex == 2 ? Sex.F : (NSex == 1 ? Sex.M : Sex.None);

		public string Post { get; set; }

		public string Subdivision { get; set; }

		public bool Dismiss { get { return DismissDate.HasValue; } }

		public DateTime? DismissDate { get; set; }
		public string Title => PersonHelper.PersonNameWithInitials(LastName, FirstName, Patronymic);
	}
}
