using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Transform;
using NHibernate.Util;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Project.Versioning;
using QS.Services;
using QS.Utilities;
using QS.Utilities.Text;
using QS.ViewModels.Resolve;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Communications;
using workwear.Repository.Regulations;
using workwear.Tools;
using workwear.ViewModels.Communications;
using workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Communications
{
	[DontUseAsDefaultViewModel]
	public class EmployeeNotificationJournalViewModel : EntityJournalViewModelBase<EmployeeCard, EmployeeViewModel, EmployeeNotificationJournalNode>
	{
		private readonly IInteractiveService interactive;
		private readonly NotificationManagerService notificationManager;
		private readonly BaseParameters baseParameters;
		private readonly IDataBaseInfo dataBaseInfo;

		public EmployeeNotificationFilterViewModel Filter { get; private set; }

		public EmployeeNotificationJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService, ILifetimeScope autofacScope, NotificationManagerService notificationManager,
			NormRepository normRepository, BaseParameters baseParameters, IDataBaseInfo dataBaseInfo,
			ICurrentPermissionService currentPermissionService = null)
										: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = false;
			Title = "Уведомление сотрудников";
			this.interactive = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			this.notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.dataBaseInfo = dataBaseInfo ?? throw new ArgumentNullException(nameof(dataBaseInfo));
			AutofacScope = autofacScope;
			JournalFilter = Filter = AutofacScope.Resolve<EmployeeNotificationFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			(DataLoader as ThreadDataLoader<EmployeeNotificationJournalNode>).PostLoadProcessingFunc = HandlePostLoadProcessing;

			//Обход проблемы с тем что SelectionMode одновременно управляет и выбором в журнале, и самим режмиом журнала.
			//То есть создает действие выбора. Удалить после того как появится рефакторинг действий журнала. 
			SelectionMode = JournalSelectionMode.Multiple;
			NodeActionsList.Clear();
			CreateActions();
		}

		void HandlePostLoadProcessing(IList items, uint addedSince)
		{
			var newItems = items.Cast<EmployeeNotificationJournalNode>().Skip((int)addedSince).ToList();
			foreach(EmployeeNotificationJournalNode item in items) 
				item.ViewModel = this;

			var statuses = notificationManager.GetStatuses(newItems.Where(x => x.LkRegistered).Select(x => x.Phone));
			foreach (var status in statuses)
			{
				var item = newItems.First(x => x.Phone == status.Phone);
				item.StatusInfo = status;
			}
		}

		protected override IQueryOver<EmployeeCard> ItemsQuery(IUnitOfWork uow)
		{
			EmployeeNotificationJournalNode resultAlias = null;

			Post postAlias = null;
			Subdivision subdivisionAlias = null;
			EmployeeCard employeeAlias = null;
			EmployeeCardItem itemAlias = null;
			ProtectionTools toolsAlias = null;
			ItemsType typesAlias = null;

			var employees = uow.Session.QueryOver(() => employeeAlias);

			DateTime startTime;
			if(Filter.ShowOverdue)
				startTime = DateTime.MinValue;
			else startTime = Filter.StartDateIssue;

			if(Filter.ShowOnlyWork)
				employees.Where(() => employeeAlias.DismissDate == null);
			if(Filter.ShowOnlyLk)
				employees.Where(() => employeeAlias.LkRegistered);
			if(Filter.Subdivision != null)
				employees.Where(() => employeeAlias.Subdivision.Id == Filter.Subdivision.Id);

			employees
				.JoinAlias(() => employeeAlias.WorkwearItems, () => itemAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);
			
				if(Filter.ContainsPeriod)
					employees = employees.Where(() => itemAlias.NextIssue >= startTime && itemAlias.NextIssue <= Filter.EndDateIssue);

			switch(Filter.IsueType) {
				case (AskIssueType.Personal):
					employees
						.JoinAlias(() => itemAlias.ProtectionTools, () => toolsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
						.JoinAlias(() => toolsAlias.Type, () => typesAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
						.Where(() => typesAlias.IssueType == IssueType.Personal);
					break;
				case (AskIssueType.Сollective):
					employees
						.JoinAlias(() => itemAlias.ProtectionTools, () => toolsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
						.JoinAlias(() => toolsAlias.Type, () => typesAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
						.Where(() => typesAlias.IssueType == IssueType.Collective);
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
					() => employeeAlias.PhoneNumber,
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
					.Select(x => x.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(x => x.DismissDate).WithAlias(() => resultAlias.DismissDate)
					.Select(x => x.PhoneNumber).WithAlias(() => resultAlias.Phone)
					.Select(x => x.LkRegistered).WithAlias(() => resultAlias.LkRegistered)
					.SelectCount(() => itemAlias.Id).WithAlias(() => resultAlias.IssueCount)
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

			var chooseAction = new JournalAction("Отметить",
					(selected) => true,
					(selected) => true
				);
			NodeActionsList.Add(chooseAction);

			var chooseSelectedAction = new JournalAction("Отметить выделенных",
					(selected) => selected.Length > 0,
					(selected) => true,
					ChooseSelected
				);
			chooseAction.ChildActionsList.Add(chooseSelectedAction);

			var chooseAllAction = new JournalAction("Отметить всех",
				(selected) => true,
				(selected) => true,
				(selected) => ChooseAll()
			);
			chooseAction.ChildActionsList.Add(chooseAllAction);

			var sendMessange = new JournalAction("Отправить сообщение",
				(selected) => Items?.Cast<EmployeeNotificationJournalNode>().Any(x => x.Selected) ?? false,
				(selected) => true,
				(selected) => SendMessange(selected)
			);
			NodeActionsList.Add(sendMessange);

			var editAction = new JournalAction("Открыть сотрудника",
					(selected) => selected.Any(),
					(selected) => VisibleEditAction,
					(selected) => selected.Cast<EmployeeNotificationJournalNode>().ToList().ForEach(EditEntityDialog)
					);

			NodeActionsList.Add(editAction);

			RowActivatedAction = editAction;
			
			var showHistoryNotificationAction = new JournalAction("Посмотреть историю уведомлений",
				(selected) => selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => ShowHistoryNotificationAction(selected)
			);

			NodeActionsList.Add(showHistoryNotificationAction);
		}

		public readonly HashSet<int> SelectedList = new HashSet<int>();

		void LoadAll()
		{
			DataLoader.DynamicLoadingEnabled = false;
			Refresh();
		}

		void SendMessange()
		{
			var ids = Items.Cast<EmployeeNotificationJournalNode>().Where(x => x.Selected).Select(x => x.Id).ToArray();
			NavigationManager.OpenViewModel<SendMessangeViewModel, int[]>(this, ids);
		}
		void ShowHistoryNotificationAction()
		{
			var ids = Items.Cast<EmployeeNotificationJournalNode>().Where(x => x.Selected).Select(x => x.Id).ToArray();
			foreach(var id in ids) {
				NavigationManager.OpenViewModel<HistoryNotificationViewModel, int>(this, id);
			}
		}

		void ChooseAll()
		{
			if (Items.Count == 0)
				return;
			
			bool setValue = !Items.Cast<EmployeeNotificationJournalNode>().Where(x => x.CanSelect).All(x => x.Selected);
			foreach (EmployeeNotificationJournalNode node in Items)
				node.Selected = node.CanSelect && setValue;
			Refresh();
		}

		void ChooseSelected(object[] nodes)
		{
			if(Items.Count == 0)
				return;

			bool setValue = !nodes.Cast<EmployeeNotificationJournalNode>().Where(x => x.CanSelect).All(x => x.Selected);
			foreach(EmployeeNotificationJournalNode node in nodes)
				node.Selected = node.CanSelect && setValue;
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
		public string Phone { get; set; }
		
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

		public bool LkRegistered { get; set; }

		public bool CanSelect => LkRegistered;

		public int IssueCount { get; set; }

		#region От сервиса уведомлений
		public UserStatusInfo StatusInfo;

		public string UnreadMessagesText => StatusInfo?.UnreadMessages > 0 ? StatusInfo.UnreadMessages.ToString() : String.Empty;
		public string LastVisit => StatusInfo?.LastVisit?.ToDateTime().ToString("g");
		
		public string StatusText {
			get {
				if (StatusInfo == null)
					return null;
				
				if (StatusInfo.Status == LkStatus.Missing)
					return "ЛК не создан";
				
				if (StatusInfo.Status == LkStatus.Registered)
					return "Нет устройств принимающих уведомления";

				if (StatusInfo.Status == LkStatus.HasTokens && StatusInfo.NotifiableDevices > 1)
					return NumberToTextRus.FormatCase(StatusInfo.NotifiableDevices,
						"{0} устройство может принять уведомление", 
						"{0} устройства может принять уведомление",
						"{0} устройств может принять уведомление");
				if (StatusInfo.Status == LkStatus.HasTokens && StatusInfo.NotifiableDevices == 1)
					return "Может принимать уведомления";
				
				return null;
			}
		}

		public string StatusColor {
			get {
				if(Dismiss)
					return "gray";

				if (StatusInfo == null)
					return null;

				switch (StatusInfo.Status)
				{
					case LkStatus.Missing: return "red";
					case LkStatus.Registered: return "blue";
					case LkStatus.HasTokens: return "green";
					default: return null;
				}
			}
		}
		#endregion
	}
}
