using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using workwear.Domain.Company;
using workwear.Domain.Operations;
using workwear.DTO;
using workwear.Repository.Operations;
using workwear.ViewModels.Stock;

namespace workwear.ViewModels.Company.EmployeeChilds
{
	public class EmployeeMovementsViewModel : ViewModelBase, IDisposable
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly EmployeeViewModel employeeViewModel;
		private readonly ITdiCompatibilityNavigation navigation;
		List<EmployeeCardMovements> movements;

		public EmployeeMovementsViewModel(EmployeeViewModel employeeViewModel, ITdiCompatibilityNavigation navigation)
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}

		#region Хелперы

		private IUnitOfWork UoW => employeeViewModel.UoW;
		private EmployeeCard Entity => employeeViewModel.Entity;

		public List<EmployeeCardMovements> Movements {
			get => movements; set => SetField(ref movements, value);
		}

		#endregion

		#region Публичные
		private bool isConfigured = false;

		public void OnShow()
		{
			if(!isConfigured) {
				isConfigured = true;
				NotifyConfiguration.Instance.BatchSubscribeOnEntity<EmployeeIssueOperation>(HandleManyEntityChangeEventMethod);
				UpdateMovements();
			}
		}

		public void OpenDoc(EmployeeCardMovements item)
		{
			if(item.ReferencedDocument == null)
				return;

			switch(item.ReferencedDocument.DocType) {
				case EmployeeIssueOpReferenceDoc.ReceivedFromStock:
					navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder, EmployeeCard>(employeeViewModel, EntityUoWBuilder.ForOpen(Entity.Id), Entity);
					break;
				case EmployeeIssueOpReferenceDoc.RetutnedToStock:
					navigation.OpenTdiTab<IncomeDocDlg, int>(employeeViewModel, item.ReferencedDocument.DocId);
					break;
				case EmployeeIssueOpReferenceDoc.WriteOff:
					navigation.OpenTdiTab<WriteOffDocDlg, int>(employeeViewModel, item.ReferencedDocument.DocId);
					break;
			}
		}

		#endregion

		#region Внутренние

		private void UpdateMovements()
		{
			logger.Info("Обновляем историю выдачи...");

			var prepareMovements = new List<EmployeeCardMovements>();

			var list = EmployeeIssueRepository.AllOperationsForEmployee(UoW, Entity, query => query.Fetch(SelectMode.Fetch, x => x.Nomenclature));
			var docs = EmployeeIssueRepository.GetReferencedDocuments(UoW, list.Select(x => x.Id).ToArray());
			foreach(var operation in list) {
				var item = new EmployeeCardMovements();
				item.Operation = operation;
				item.ReferencedDocument = docs.FirstOrDefault(x => x.OpId == operation.Id);
				item.PropertyChanged += Item_PropertyChanged;
				prepareMovements.Add(item);
			}
			Movements = prepareMovements;

			logger.Info("Ок");
		}

		void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == PropertyUtil.GetName<EmployeeCardMovements>(x => x.UseAutoWriteOff)) {
				var item = sender as EmployeeCardMovements;
				UoW.Save(item.Operation);
			}
		}

		#endregion

		void HandleManyEntityChangeEventMethod(EntityChangeEvent[] changeEvents)
		{
			var updatedOperations = changeEvents.Where(x => x.GetEntity<EmployeeIssueOperation>().Employee.IsSame(employeeViewModel.Entity)).ToList();
			if(updatedOperations.Count > 0) {
				Movements.ForEach(m => UoW.Session.Evict(m.Operation));
				UpdateMovements();
			}
		}

		public void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
		}
	}
}
