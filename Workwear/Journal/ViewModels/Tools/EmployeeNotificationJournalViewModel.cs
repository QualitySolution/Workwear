using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Transform;
using NHibernate.Util;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Project.Versioning;
using QS.Services;
using QS.Utilities.Text;
using QS.ViewModels.Resolve;
using workwear.Domain.Company;
using workwear.Journal.Filter.ViewModels.Tools;
using workwear.Repository.Regulations;
using workwear.Tools;
using workwear.ViewModels.Company;
using workwear.ViewModels.Tools;

namespace workwear.Journal.ViewModels.Tools
{
	[DontUseAsDefaultViewModel]
	public class EmployeeNotificationJournalViewModel : EntityJournalViewModelBase<EmployeeCard, EmployeeViewModel, EmployeeNotificationJournalNode>
	{
		private readonly IInteractiveService interactive;
		private readonly BaseParameters baseParameters;
		private readonly IDataBaseInfo dataBaseInfo;

		public NotificationFilterViewModel Filter { get; private set; }

		public EmployeeNotificationJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService, ILifetimeScope autofacScope,
			NormRepository normRepository, BaseParameters baseParameters, IDataBaseInfo dataBaseInfo,
			ICurrentPermissionService currentPermissionService = null)
										: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = false;
			Title = "Уведомление сотрудников";
			this.interactive = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.dataBaseInfo = dataBaseInfo ?? throw new ArgumentNullException(nameof(dataBaseInfo));
			AutofacScope = autofacScope;
			JournalFilter = Filter = AutofacScope.Resolve<NotificationFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			(DataLoader as ThreadDataLoader<EmployeeNotificationJournalNode>).PostLoadProcessingFunc = delegate(IList items, uint addedSince) {
				foreach(EmployeeNotificationJournalNode item in items)
					item.ViewModel = this;
			};

			//Обход проблемы с тем что SelectionMode одновременно управляет и выбором в журнале, и самим режмиом журнала.
			//То есть создает действие выбора. Удалить после того как появится рефакторинг действий журнала. 
			SelectionMode = JournalSelectionMode.Multiple;
			NodeActionsList.Clear();
			CreateActions();
		}

		protected override IQueryOver<EmployeeCard> ItemsQuery(IUnitOfWork uow)
		{
			EmployeeNotificationJournalNode resultAlias = null;

			Post postAlias = null;
			Subdivision subdivisionAlias = null;
			EmployeeCard employeeAlias = null;
			EmployeeCardItem itemAlias = null;

			var employees = uow.Session.QueryOver(() => employeeAlias);

			DateTime startTime;
			if(Filter.ShowOverdue)
				startTime = DateTime.MinValue;
			else startTime = Filter.StartDateIssue;

			if(Filter.ShowOnlyWork)
				employees.Where(() => employeeAlias.DismissDate == null);
			if(Filter.Subdivision != null)
				employees.Where(() => employeeAlias.Subdivision.Id == Filter.Subdivision.Id);

			employees
				.JoinAlias(() => employeeAlias.WorkwearItems, () => itemAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
					.Where(() => itemAlias.NextIssue >= startTime && itemAlias.NextIssue <= Filter.EndDateIssue);

			switch(Filter.IsueType) {
				case (AskIssueType.All):
					break;
				case (AskIssueType.Personal):
					break;
				case (AskIssueType.Сollective):
					break;
			}


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
				.SelectList((list) => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.CardNumber).WithAlias(() => resultAlias.CardNumber)
					.Select(x => x.PersonnelNumber).WithAlias(() => resultAlias.PersonnelNumber)
					.Select(x => x.FirstName).WithAlias(() => resultAlias.FirstName)
					.Select(x => x.LastName).WithAlias(() => resultAlias.LastName)
					.Select(() => employeeAlias.DismissDate).WithAlias(() => resultAlias.DismissDate)
					.Select(x => x.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => postAlias.Name).WithAlias(() => resultAlias.Post)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					)
				.OrderBy(() => employeeAlias.LastName).Asc
				.ThenBy(() => employeeAlias.FirstName).Asc
				.ThenBy(() => employeeAlias.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<EmployeeNotificationJournalNode>());
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

			var editAction = new JournalAction("Открыть сотрудника",
					(selected) => selected.Any(),
					(selected) => VisibleEditAction,
					(selected) => selected.Cast<EmployeeNotificationJournalNode>().ToList().ForEach(EditEntityDialog)
					);

			NodeActionsList.Add(editAction);

			RowActivatedAction = editAction;

			var sendMessange = new JournalAction("Отправить сообщение",
					(selected) => SelectedList.Count !=0,
					(selected) => true,
					(selected) => SendMessange()
					);
			NodeActionsList.Add(sendMessange);

			var invertSelected = new JournalAction("Выделить/Снять выделение",
					(selected) => true,
					(selected) => true,
					(selected) => InvertSelected()
				);
			NodeActionsList.Add(invertSelected);
		}

		public readonly HashSet<int> SelectedList = new HashSet<int>();

		void LoadAll()
		{
			DataLoader.DynamicLoadingEnabled = false;
			Refresh();
		}

		void SendMessange()
		{
			NavigationManager.OpenViewModel<SendMessangeViewModel>(this);
		}

		void InvertSelected()
		{
			if (Items.Count == 0)
				return;
			
			bool setValue = !(Items[0] as EmployeeNotificationJournalNode).Selected;
			foreach (EmployeeNotificationJournalNode node in Items)
				node.Selected = setValue;
			Refresh();
		}
		#endregion
	}

	public class EmployeeNotificationJournalNode
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

		public EmployeeNotificationJournalViewModel ViewModel;

		public bool Selected {
			get => ViewModel.SelectedList.Contains(Id);
			set { if(value)
					ViewModel.SelectedList.Add(Id);
				else
					ViewModel.SelectedList.Remove(Id);
			}
		}
		//true это затычка пока не готова серверная часть
		public bool CanSelect { get; set; } = true;

		public string Result { get; set; }

		public string PersonalAccountStatus { get; set; }

		public DateTime? DateLastVisit { get; set; }

		public string LastVisit {
			get {
				return DateLastVisit == null ? "" : DateLastVisit.Value.ToString("g");
			}
		}
	}
}
