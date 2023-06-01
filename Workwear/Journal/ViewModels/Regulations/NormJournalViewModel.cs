using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Journal.Filter.ViewModels.Regulations;
using workwear.Journal.ViewModels.Company;
using Workwear.Repository.Company;
using Workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations
{
	public class NormJournalViewModel : EntityJournalViewModelBase<Norm, NormViewModel, NormJournalNode>
	{
		private readonly ILifetimeScope autofacScope;
		public NormFilterViewModel Filter { get; private set; }

		public NormJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, ILifetimeScope autofacScope, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null, bool useMultiSelect = false) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			UseSlider = false;
			JournalFilter = Filter = autofacScope.Resolve<NormFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
			CreatePopupActions();
			if(useMultiSelect)
				UseMultiSelect();
		}

		protected override IQueryOver<Norm> ItemsQuery(IUnitOfWork uow)
		{
			NormJournalNode resultAlias = null;

			Post postAlias = null;
			Subdivision subdivisionAlias = null;
			Department departmentAlias = null;
			Norm normAlias = null;
			NormItem normItemAlias = null;
			RegulationDoc regulationDocAlias = null;
			RegulationDocAnnex docAnnexAlias = null;
			EmployeeCard employeeAlias = null;
			Norm usedNormAlias = null;

			var employeesSubquery = QueryOver.Of<EmployeeCard>(() => employeeAlias)
				.JoinQueryOver(e => e.UsedNorms, () => usedNormAlias)
				.Where(() => usedNormAlias.Id == normAlias.Id)
				.ToRowCountQuery();

			var employeesWorkedSubquery = QueryOver.Of<EmployeeCard>(() => employeeAlias)
				.Where(e => e.DismissDate == null)
				.JoinQueryOver(e => e.UsedNorms, () => usedNormAlias)
				.Where(() => usedNormAlias.Id == normAlias.Id)
				.ToRowCountQuery();


			var norms = uow.Session.QueryOver<Norm>(() => normAlias)
				.JoinAlias(n => n.Document, () => regulationDocAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(n => n.Annex, () => docAnnexAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => normAlias.Posts, () => postAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => postAlias.Subdivision, () => subdivisionAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => postAlias.Department, () => departmentAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(GetSearchCriterion(
					() => normAlias.Name,
					() => postAlias.Name,
					() => subdivisionAlias.Name,
					() => departmentAlias.Name,
					() => normAlias.Id
					));
			if (Filter.Post != null)
				norms.Where(() => postAlias.Id == Filter.Post.Id);

			if(Filter.ProtectionTools != null)
				norms.JoinAlias(n => n.Items, () => normItemAlias)
					.Where(() => normItemAlias.ProtectionTools.Id == Filter.ProtectionTools.Id);

			if(Filter.Subdivision != null)
				norms.Where(() => subdivisionAlias.Id == Filter.Subdivision.Id);

			return norms
				.SelectList(list => list
				   .SelectGroup(() => normAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => regulationDocAlias.Number).WithAlias(() => resultAlias.TonNumber)
				   .Select(() => docAnnexAlias.Number).WithAlias(() => resultAlias.AnnexNumber)
				   .Select(() => normAlias.TONParagraph).WithAlias(() => resultAlias.TonParagraph)
				   .Select(() => normAlias.Name).WithAlias(() => resultAlias.Name)
				   .SelectSubQuery(employeesSubquery).WithAlias(() => resultAlias.Usages)
				   .SelectSubQuery(employeesWorkedSubquery).WithAlias(() => resultAlias.UsagesWorked)
				   .Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String,
						"GROUP_CONCAT( CONCAT( ?1, IF(?2 IS NULL AND ?3 IS NULL, '', CONCAT(' [', CONCAT_WS('›', ?2, ?3), ']'))) SEPARATOR ?4)"),
					   NHibernateUtil.String,
					   Projections.Property(() => postAlias.Name),
					   Projections.Property(() => subdivisionAlias.Name),
					   Projections.Property(() => departmentAlias.Name),
					   Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.Posts)
				)
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NormJournalNode>());
		}

		public void UseMultiSelect()
		{
			//Обход проблемы с тем что SelectionMode одновременно управляет и выбором в журнале, и самим режимом журнала.
			//То есть создает действие выбора. Удалить после того как появится рефакторинг действий журнала. 
			SelectionMode = JournalSelectionMode.Multiple;
			NodeActionsList.RemoveAll(x => x.Title == "Выбрать");
			RowActivatedAction = NodeActionsList.First(x => x.Title == "Изменить");
			NodeActionsList.Add(new JournalAction("Обновить потребности",
				(nodes) => nodes.Cast<NormJournalNode>().Any(x => x.UsagesWorked > 0),
				(arg) => true,
				UpdateWearItems));
		}

		#region Popupmenu action implementation
		protected override void CreatePopupActions()
		{
			PopupActionsList.Add(new JournalAction("Копировать норму", (arg) => arg.Length == 1, (arg) => true, CopyNorm));
			PopupActionsList.Add(new JournalAction("Сотрудники использующие норму", (arg) => arg.Length == 1, (arg) => true, ShowEmployees));
			PopupActionsList.Add(new JournalAction("Обновить потребности у использующих норму", 
				(nodes) => nodes.Cast<NormJournalNode>().Any(x => x.UsagesWorked > 0),
				(arg) => true,
				UpdateWearItems));
		}

		private void CopyNorm(object[] nodes)
		{
			if(nodes.Length != 1)
				return;
			int normId = (nodes[0] as NormJournalNode).Id;
			var page = NavigationManager.OpenViewModel<NormViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());
			page.ViewModel.CopyNormFrom(normId);
		}

		private void ShowEmployees(object[] nodes)
		{
			foreach(NormJournalNode node in nodes) {
				NavigationManager.OpenViewModel<EmployeeJournalViewModel, Norm>(this, new Norm {Id = node.Id}, OpenPageOptions.IgnoreHash); //Фейковая норма для передачи id
			}
		}

		void UpdateWearItems(object[] nodes)
		{
			var progressPage = NavigationManager.OpenViewModel<ProgressWindowViewModel>(null);
			var progress = progressPage.ViewModel.Progress;

			using(var localUow = UnitOfWorkFactory.CreateWithoutRoot("Обновление потребностей из журнала норм")) {
				var employeeRepository = autofacScope.Resolve<EmployeeRepository>(new TypedParameter(typeof(IUnitOfWork), localUow));
				progress.Start(2, text: "Загружаем нормы");
				var norms = localUow.GetById<Norm>(nodes.GetIds()).ToArray();
				progress.Add(text: "Загружаем сотрудников");
				var employees = employeeRepository.GetEmployeesUseNorm(norms);

				progress.Start(employees.Count + 1);
				int step = 0;
				foreach(var employee in employees) {
					progress.Add(text: $"Обработка {employee.ShortName}");
					step++;
					employee.UpdateWorkwearItems();
					localUow.Save(employee);
					if(step % 10 == 0)
						localUow.Commit();
				}
				progress.Add(text: "Завершаем...");
				localUow.Commit();
			}
			NavigationManager.ForceClosePage(progressPage, CloseSource.FromParentPage);
		}
		#endregion
	}

	public class NormJournalNode
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string TonNumber { get; set; }

		public int? AnnexNumber { get; set; }

		public string TonAttachment => AnnexNumber?.ToString();

		public string TonParagraph { get; set; }

		public string Posts { get; set; }

		public int Usages { get; set; }

		public int UsagesWorked { get; set; }

		public string UsageText => Usages == UsagesWorked ? Usages.ToString() : $"{Usages}({UsagesWorked})";
		public string UsageToolTip => Usages == UsagesWorked ? totalUsage : totalUsage + workedUsage;

		private string totalUsage => NumberToTextRus.FormatCase(Usages, "Применена к {0} сотруднику", "Применена к {0} сотрудникам", "Применена к {0} сотрудникам");
		private string workedUsage => NumberToTextRus.FormatCase(UsagesWorked, " (из них {0} работающий)", " (из них {0} работающих)", " (из них {0} работающих)");
	}
}
