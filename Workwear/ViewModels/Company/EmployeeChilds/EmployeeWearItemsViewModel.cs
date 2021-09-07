﻿using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using workwear.Dialogs.Issuance;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Repository.Operations;
using workwear.Tools;
using workwear.ViewModels.Stock;

namespace workwear.ViewModels.Company.EmployeeChilds
{
	public class EmployeeWearItemsViewModel : ViewModelBase, IDisposable
	{
		private readonly EmployeeViewModel employeeViewModel;
		private readonly EmployeeIssueRepository employeeIssueRepository;
		private readonly BaseParameters baseParameters;
		private readonly IInteractiveQuestion interactive;
		private readonly ITdiCompatibilityNavigation navigation;

		public EmployeeWearItemsViewModel(EmployeeViewModel employeeViewModel, EmployeeIssueRepository employeeIssueRepository, BaseParameters baseParameters, IInteractiveQuestion interactive, ITdiCompatibilityNavigation navigation)
		{
			Contract.Requires(interactive != null);
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.employeeIssueRepository = employeeIssueRepository ?? throw new ArgumentNullException(nameof(employeeIssueRepository));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));

			employeeIssueRepository.RepoUow = UoW;
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<EmployeeCardItem>(HandleEntityChangeEvent);
		}

		#region Хелперы

		private IUnitOfWork UoW => employeeViewModel.UoW;
		private EmployeeCard Entity => employeeViewModel.Entity;

		#endregion

		#region Показ
		private bool isConfigured = false;

		public void OnShow()
		{
			if(!isConfigured) {
				isConfigured = true;
				Entity.FillWearInStockInfo(UoW, baseParameters, Entity.Subdivision?.Warehouse, DateTime.Now);
				Entity.FillWearRecivedInfo(employeeIssueRepository);
				OnPropertyChanged(nameof(ObservableWorkwearItems));
			}
		}

		#endregion

		#region Свойства

		public GenericObservableList<EmployeeCardItem> ObservableWorkwearItems => Entity.ObservableWorkwearItems;

		#endregion

		#region Внутренне
		void HandleEntityChangeEvent(EntityChangeEvent[] changeEvents)
		{
			if(!isConfigured)
				return;
			if(changeEvents.First().Session == UoW.Session)
				return; //Не чего не делаем если это наше собственное изменение.
			if(changeEvents.Where(x => x.EventType == TypeOfChangeEvent.Delete).Select(e => e.Entity).OfType<EmployeeCardItem>().Any(x => x.EmployeeCard.IsSame(Entity))) {
				//Если сделано удаление строк, просто закрываем диалог, так как заставить корректно сохранить сотрудника все равно не поучится.
				//Не работаел следующий сценарий: Открываем диалог сотрудника, строка добавленая по норме есть в списке, открываем норму, удаляем одну из строк, сохраняем норму. После этого пытаемся сохранить сотрудника.
				var page = navigation.FindPage(employeeViewModel);
				navigation.ForceClosePage(page, CloseSource.Self);
				return;
			}

			if(changeEvents.Select(e => e.Entity).OfType<EmployeeCardItem>().Any(x => x.EmployeeCard.IsSame(Entity))) {
				RefreshWorkItems();
			}
		}

		#endregion

		#region Действия View

		public void GiveWearByNorm()
		{
			if(!employeeViewModel.Save())
				return;
			navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder, EmployeeCard>(employeeViewModel, EntityUoWBuilder.ForCreate(), Entity);
		}

		public void ReturnWear()
		{
			navigation.OpenTdiTab<IncomeDocDlg, EmployeeCard>(employeeViewModel, Entity);
		}

		public void OpenTimeLine(EmployeeCardItem item)
		{
			navigation.OpenTdiTab<EmployeeIssueGraphDlg, EmployeeCard, ProtectionTools>(employeeViewModel, Entity, item.ProtectionTools);
		}

		public void WriteOffWear()
		{
			navigation.OpenTdiTab<WriteOffDocDlg, EmployeeCard>(employeeViewModel, Entity);
		}

		public void UpdateWorkwearItems()
		{
			Entity.UpdateWorkwearItems();
		}

		#endregion
		protected void RefreshWorkItems()
		{
			if(!NHibernateUtil.IsInitialized(Entity.WorkwearItems))
				return;

			foreach(var item in Entity.WorkwearItems) {
				UoW.Session.Refresh(item);
			}
			Entity.FillWearInStockInfo(UoW, baseParameters, Entity.Subdivision?.Warehouse, DateTime.Now);
			Entity.FillWearRecivedInfo(employeeIssueRepository);
		}

		public void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
		}
	}
}