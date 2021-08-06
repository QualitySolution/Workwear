using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Journal.Filter.ViewModels.Company;
using workwear.Repository.Regulations;
using workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Tools
{
	public class EmployeeSetNormViewModel : EntityJournalViewModelBase<EmployeeCard, EmployeeViewModel, EmployeeSetNormJournalNode>
	{
		private readonly NormRepository normRepository;

		public EmployeeFilterViewModel Filter { get; private set; }

		public EmployeeSetNormViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, 
										IDeleteEntityService deleteEntityService, ILifetimeScope autofacScope, NormRepository normRepository, ICurrentPermissionService currentPermissionService = null) 
										: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = false;

			this.normRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));

			AutofacScope = autofacScope;
			JournalFilter = Filter = AutofacScope.Resolve<EmployeeFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			//Обход проблемы с тем что SelectionMode одновременно управляет и выбором в журнале, и самим режмиом журнала.
			//То есть создает действие выбора. Удалить после того как появится рефакторинг действий журнала. 
			SelectionMode = JournalSelectionMode.Multiple;
			NodeActionsList.Clear();
			CreateActions();

			(DataLoader as ThreadDataLoader<EmployeeSetNormJournalNode>).PostLoadProcessingFunc = delegate (System.Collections.IList items, uint addedSince) {
				foreach(EmployeeSetNormJournalNode item in items) {
					if(Results.ContainsKey(item.Id))
						item.Result = Results[item.Id];
				}
			};
		}

		protected override IQueryOver<EmployeeCard> ItemsQuery(IUnitOfWork uow)
		{
			EmployeeSetNormJournalNode resultAlias = null;

			Post postAlias = null;
			Subdivision subdivisionAlias = null;
			EmployeeCard employeeAlias = null;
			Norm normAlias = null;


			var employees = uow.Session.QueryOver<EmployeeCard>(() => employeeAlias);
			if(Filter.ShowOnlyWork)
				employees.Where(x => x.DismissDate == null);
			if(Filter.Subdivision != null)
				employees.Where(x => x.Subdivision.Id == Filter.Subdivision.Id);
			if(Filter.Department != null)
				employees.Where(x => x.Department.Id == Filter.Department.Id);

			var normProjection = CustomProjections.GroupConcat(Projections.SqlFunction("coalesce", NHibernateUtil.String, Projections.Property(() => normAlias.Name), Projections.Property(() => normAlias.Id)), separator: "\n");

			return employees
				.Where(GetSearchCriterion(
					() => employeeAlias.Id,
					() => employeeAlias.CardNumber,
					() => employeeAlias.PersonnelNumber,
					() => employeeAlias.LastName,
					() => employeeAlias.FirstName,
					() => employeeAlias.Patronymic,
					() => postAlias.Name,
					() => subdivisionAlias.Name
 					))
				.JoinAlias(() => employeeAlias.Post, () => postAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeAlias.Subdivision, () => subdivisionAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => employeeAlias.UsedNorms, () => normAlias)
				.SelectList((list) => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.CardNumber).WithAlias(() => resultAlias.CardNumber)
					.Select(x => x.PersonnelNumber).WithAlias(() => resultAlias.PersonnelNumber)
					.Select(x => x.FirstName).WithAlias(() => resultAlias.FirstName)
					.Select(x => x.LastName).WithAlias(() => resultAlias.LastName)
					.Select(x => x.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => employeeAlias.DismissDate).WithAlias(() => resultAlias.DismissDate)
					.Select(() => postAlias.Name).WithAlias(() => resultAlias.Post)
	   				.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					.Select(normProjection).WithAlias(() => resultAlias.Norms)
 					)
				.OrderBy(() => employeeAlias.LastName).Asc
				.ThenBy(() => employeeAlias.FirstName).Asc
				.ThenBy(() => employeeAlias.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<EmployeeSetNormJournalNode>());
		} 

		#region Действия
		void CreateActions()
		{
			var loadAllAction = new JournalAction("Загрузить всех",
					(selected) => true,
					(selected) => true,
					(selected) => LoadAll()
					);
			NodeActionsList.Add(loadAllAction);

			var updateStatusAction = new JournalAction("Установить по должности",
					(selected) => selected.Any(),
					(selected) => true,
					(selected) => UpdateNorms(selected.Cast<EmployeeSetNormJournalNode>().ToArray())
					);
			NodeActionsList.Add(updateStatusAction);
		}

		private Dictionary<int, string> Results = new Dictionary<int, string>();

		void UpdateNorms(EmployeeSetNormJournalNode[] nodes)
		{
			var progressPage = NavigationManager.OpenViewModel<ProgressWindowViewModel>(null);
			var progress = progressPage.ViewModel.Progress;

			progress.Start(nodes.Length + 2, text: "Загружаем сотрудников");
			var employees = UoW.GetById<EmployeeCard>(nodes.Select(x => x.Id));
			progress.Add(text: "Загружаем нормы");
			var norms = normRepository.GetNormsForPost(UoW, employees.Select(x => x.Post).Where(x => x != null).Distinct().ToArray());

			int step = 0;

			foreach(var employee in employees) {
				progress.Add(text: $"Обработка {employee.ShortName}");
				if(employee.Post == null) {
					Results.Add(employee.Id, "Отсутствует должность");
					continue;
				}
				var norm = norms.FirstOrDefault(x => x.IsActive && x.Posts.Contains(employee.Post));
				if(norm != null) {
					step++;
					employee.UsedNorms.Clear();
					employee.AddUsedNorm(norm);
					UoW.Save(employee);
					Results.Add(employee.Id, "ОК");
					if(step % 10 == 0)
						UoW.Commit();
				}
				else {
					Results.Add(employee.Id, "Подходящая норма не найдена");
				}
			}
			progress.Add(text: "Готово");
			UoW.Commit();
			NavigationManager.ForceClosePage(progressPage, CloseSource.FromParentPage);
			Refresh();
		}

		void LoadAll()
		{
			DataLoader.DynamicLoadingEnabled = false;
			Refresh();
		}
		#endregion
	}

	public class EmployeeSetNormJournalNode
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
		[SearchHighlight]
		public string Post { get; set; }
		[SearchHighlight]
		public string Subdivision { get; set; }

		public bool Dismiss { get { return DismissDate.HasValue; } }

		public DateTime? DismissDate { get; set; }
		public string Title => PersonHelper.PersonNameWithInitials(LastName, FirstName, Patronymic);

		public string Norms { get; set; }

		public string Result { get; set; }
	}
}
