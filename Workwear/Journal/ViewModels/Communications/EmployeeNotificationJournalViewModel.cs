﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Gamma.ColumnConfig;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Cloud.WearLk.Client;
using QS.Cloud.WearLk.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Services;
using QS.Utilities;
using QS.Utilities.Text;
using QS.ViewModels.Resolve;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using workwear.Journal.Filter.ViewModels.Communications;
using Workwear.Models.Operations;
using Workwear.Tools;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Communications;
using Workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Communications
{
	[DontUseAsDefaultViewModel]
	public class EmployeeNotificationJournalViewModel : EntityJournalViewModelBase<EmployeeCard, EmployeeViewModel, EmployeeNotificationJournalNode>
	{
		private readonly UnitOfWorkProvider unitOfWorkProvider;
		private readonly StockBalanceModel stockBalanceModel;
		private readonly EmployeeIssueModel issueModel;
		private readonly BaseParameters baseParameters;
		private readonly NotificationManagerService notificationManager;
		private readonly SizeService sizeService;
		private bool alreadyLoaded;
		
		public EmployeeNotificationFilterViewModel Filter { get; private set; }

		public EmployeeNotificationJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService, INavigationManager navigationManager,
			UnitOfWorkProvider unitOfWorkProvider, IDeleteEntityService deleteEntityService,
			ILifetimeScope autofacScope, StockBalanceModel stockBalanceModel, EmployeeIssueModel issueModel,
			BaseParameters baseParameters, NotificationManagerService notificationManager, 
			SizeService sizeService, ICurrentPermissionService currentPermissionService = null)
										: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = false;
			Title = "Уведомление сотрудников";
			this.unitOfWorkProvider = unitOfWorkProvider ?? throw new ArgumentNullException(nameof(unitOfWorkProvider));
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.issueModel = issueModel ?? throw new ArgumentNullException(nameof(issueModel));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			JournalFilter = Filter = autofacScope.Resolve<EmployeeNotificationFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			this.unitOfWorkProvider.UoW = UoW;
			
			(DataLoader as ThreadDataLoader<EmployeeNotificationJournalNode>).PostLoadProcessingFunc = HandlePostLoadProcessing;

			TableSelectionMode = JournalSelectionMode.Multiple;
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
			
			if (Filter.ContainsPeriod && Filter.CheckInStockAvailability) 
			{
				stockBalanceModel.Warehouse = (Filter.SelectedWarehouse.Id == -1) ? null : Filter.SelectedWarehouse;
				IEnumerable<EmployeeCard> employees = issueModel.PreloadEmployeeInfo(newItems.Select(x => x.Id).ToArray());
				LoadSizes();
				issueModel.PreloadWearItems(employees.Select(x => x.Id).ToArray());
				issueModel.FillWearInStockInfo(employees, stockBalanceModel);
				issueModel.FillWearReceivedInfo(employees.ToArray());
				foreach (EmployeeCard employee in employees) 
				{
					IEnumerable<EmployeeCardItem> cardtems = employee.GetUnderreceivedItems(baseParameters, Filter.EndDateIssue)
						.Where(x => Filter.SelectedProtectionToolsIds.Contains(x.ProtectionTools.Id));
					if (cardtems.All(x => x.InStock.Sum(c => c.Amount) == 0)) 
					{
						for (int i = 0; i < items.Count; i++) 
						{
							if ((items[i] as EmployeeNotificationJournalNode).Id == employee.Id) 
							{
								items.RemoveAt(i);
								break;
							}
						}
					}
				}
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
			if (Filter.ContainsPeriod) 
			{
				employees = employees
					.Where(() => itemAlias.ProtectionTools.Id.IsIn(Filter.SelectedProtectionToolsIds))
					.Where(() => itemAlias.NextIssue >= startTime && itemAlias.NextIssue <= Filter.EndDateIssue);
			}

			if(Filter.ContainsDateBirthPeriod)
				if (Filter.StartDateBirth.Month <= Filter.EndDateBirth.Month) {
					var projection = Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.DateTime,
							"(DATE_FORMAT(?1,'%m%d%h%i') between DATE_FORMAT(?2,'%m%d%h%i') and DATE_FORMAT(?3,'%m%d%h%i'))"),
						NHibernateUtil.DateTime,
						Projections.Property(() => employeeAlias.BirthDate),
						Projections.Constant(Filter.StartDateBirth),
						Projections.Constant(Filter.EndDateBirth)
					);
					employees.Where(x => x.BirthDate != null).And(Restrictions.Eq(projection, true));
				}
				else {
					//когда в период попадает переход между годами
					var projection = Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.DateTime,
							"(DATE_FORMAT(?1,'%m%d%h%i') between DATE_FORMAT(?2,'%m%d%h%i') and DATE_FORMAT(?4,'%m%d%h%i') " +
							"OR DATE_FORMAT(?1,'%m%d%h%i') between DATE_FORMAT(?5,'%m%d%h%i') and DATE_FORMAT(?3,'%m%d%h%i'))"),
						NHibernateUtil.DateTime,
						Projections.Property(() => employeeAlias.BirthDate),
						Projections.Constant(Filter.StartDateBirth),
						Projections.Constant(Filter.EndDateBirth),
						Projections.Constant(new DateTime(1, 12, 31)),
						Projections.Constant(new DateTime(1, 1, 1))
					);
					employees.Where(x => x.BirthDate != null).And(Restrictions.Eq(projection, true));
				}

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

			employees
				.Where(GetSearchCriterion(
					() => employeeAlias.Id,
					() => employeeAlias.CardNumber,
					() => employeeAlias.PersonnelNumber,
					() => employeeAlias.LastName,
					() => employeeAlias.FirstName,
					() => employeeAlias.Patronymic,
					() => employeeAlias.PhoneNumber,
					() => employeeAlias.Email,
					() => postAlias.Name,
					() => subdivisionAlias.Name
				))
				.JoinAlias(() => employeeAlias.Post, () => postAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeAlias.Subdivision, () => subdivisionAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
					.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.CardNumber).WithAlias(() => resultAlias.CardNumber)
					.Select(x => x.PersonnelNumber).WithAlias(() => resultAlias.PersonnelNumber)
					.Select(x => x.FirstName).WithAlias(() => resultAlias.FirstName)
					.Select(x => x.LastName).WithAlias(() => resultAlias.LastName)
					.Select(x => x.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(x => x.DismissDate).WithAlias(() => resultAlias.DismissDate)
					.Select(x => x.PhoneNumber).WithAlias(() => resultAlias.Phone)
					.Select(x => x.Email).WithAlias(() => resultAlias.Email)
					.Select(x => x.LkRegistered).WithAlias(() => resultAlias.LkRegistered)
					.SelectCount(() => itemAlias.Id).WithAlias(() => resultAlias.IssueCount)
					.Select(() => postAlias.Name).WithAlias(() => resultAlias.Post)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					.Select(x => x.BirthDate).WithAlias(() => resultAlias.BirthDate)
				);
			if (Filter.ContainsDateBirthPeriod) {
				if (Filter.StartDateBirth.Month <= Filter.EndDateBirth.Month)
					employees = employees
						.OrderBy(() => employeeAlias.BirthDate.Value.Month).Asc;
				else
					employees = employees
						.OrderBy(() => employeeAlias.BirthDate.Value.Month).Desc;
				employees = employees
					.ThenBy(() => employeeAlias.BirthDate.Value.Day).Asc
					.ThenBy(() => employeeAlias.LastName).Asc
					.ThenBy(() => employeeAlias.FirstName).Asc
					.ThenBy(() => employeeAlias.Patronymic).Asc;
			}
			else
				employees = employees.OrderBy(() => employeeAlias.LastName).Asc
					.ThenBy(() => employeeAlias.FirstName).Asc
					.ThenBy(() => employeeAlias.Patronymic).Asc;

			return employees.TransformUsing(Transformers.AliasToBean<EmployeeNotificationJournalNode>());
		}

		#region Действия
		void CreateActions()
		{
			NodeActionsList.Clear();
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

			var sendMessage = new JournalAction("Отправить сообщение",
				(selected) => Items?.Cast<EmployeeNotificationJournalNode>().Any(x => x.Selected) ?? false,
				(selected) => true,
				(selected) => SendMessage(selected)
			);
			NodeActionsList.Add(sendMessage);

			var editAction = new JournalAction("Открыть сотрудника",
					(selected) => selected.Any(),
					(selected) => VisibleEditAction,
					(selected) => selected.Cast<EmployeeNotificationJournalNode>().ToList().ForEach(EditEntityDialog)
					);

			NodeActionsList.Add(editAction);

			RowActivatedAction = editAction;
			
			var showHistoryNotificationAction = new JournalAction("Показать сообщения",
				(selected) => selected.Any(x => ((EmployeeNotificationJournalNode) x).CanSandNotification),
				(selected) => VisibleEditAction,
				(selected) => ShowHistoryNotificationAction(selected)
			);

			NodeActionsList.Add(showHistoryNotificationAction);

			var copyNumbers = new JournalAction("Скопировать телефоны",
				(selected) => selected
					.Cast<EmployeeNotificationJournalNode>()
					.Any(x => !String.IsNullOrEmpty(x.Phone)),
				(selected) => true,
				(selected) => CopyNumbers(selected)
			);
			NodeActionsList.Add(copyNumbers);
		}

		public readonly HashSet<int> SelectedList = new HashSet<int>();

		void LoadAll()
		{
			DataLoader.DynamicLoadingEnabled = false;
			Refresh();
		}

		void SendMessage(object[] nodes) 
		{
			var ids = Items.Cast<EmployeeNotificationJournalNode>().Where(x => x.Selected).Select(x => x.Id).ToArray();
			DateTime? endDateIssue = null;
			if (Filter.ContainsPeriod) 
			{
				endDateIssue = Filter.EndDateIssue;
			}

			NavigationManager.OpenViewModelNamedArgs<SendMessangeViewModel>(this, new Dictionary<string, object> {
				{ "employeeIds", ids },
				{ "warehouseId", Filter.SelectedWarehouse.Id },
				{ "endDateIssue", endDateIssue },
				{ "protectionToolsIds", Filter.SelectedProtectionToolsIds.ToArray() }
			});
		}
		void ShowHistoryNotificationAction(object[] nodes)
		{
			var employeeNodes = nodes.Cast<EmployeeNotificationJournalNode>();
			foreach(var node in employeeNodes) {
				if(node.CanSandNotification)
					NavigationManager.OpenViewModel<HistoryNotificationViewModel, int>(this, node.Id);
			}
		}

		void ChooseAll()
		{
			if (Items.Count == 0)
				return;
			
			bool setValue = !Items.Cast<EmployeeNotificationJournalNode>().Where(x => x.CanSandNotification).All(x => x.Selected);
			foreach (EmployeeNotificationJournalNode node in Items)
				node.Selected = node.CanSandNotification && setValue;
			Refresh();
		}

		void ChooseSelected(object[] nodes)
		{
			if(Items.Count == 0)
				return;

			bool setValue = !nodes.Cast<EmployeeNotificationJournalNode>().Where(x => x.CanSandNotification).All(x => x.Selected);
			foreach(EmployeeNotificationJournalNode node in nodes)
				node.Selected = node.CanSandNotification && setValue;
			Refresh();
		}
		
		public void CopyNumbers(object[] nodes) {
			var clipboard = Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
			var numbers = nodes.Cast<EmployeeNotificationJournalNode>()
				.Select(x => x.Phone).ToArray();
			var numbersText = String.Join("\n", numbers);
			clipboard.Text = numbersText;
			clipboard.Store();
		}
		
		private void LoadSizes() 
		{
			if (!alreadyLoaded) 
			{
				sizeService.RefreshSizes(UoW);
				alreadyLoaded = true;
			}
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
		public string Email { get; set; }
		
		[SearchHighlight]
		public string Post { get; set; }
		[SearchHighlight]
		public string Subdivision { get; set; }

		public bool Dismiss { get { return DismissDate.HasValue; } }

		public DateTime? DismissDate { get; set; }
		public DateTime? BirthDate { get; set; }

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

		public bool CanSandNotification => LkRegistered || !string.IsNullOrWhiteSpace(Email);

		public int IssueCount { get; set; }

		#region От сервиса уведомлений
		public UserStatusInfo StatusInfo;

		public string UnreadMessagesText => StatusInfo?.UnreadMessages > 0 ? StatusInfo.UnreadMessages.ToString() : String.Empty;
		public string LastVisit => StatusInfo?.LastVisit?.ToDateTime().ToLocalTime().ToString("g");
		
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
