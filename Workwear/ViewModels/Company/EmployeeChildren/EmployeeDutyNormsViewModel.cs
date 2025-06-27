using System;
using System.ComponentModel;
using NPOI.SS.Formula.Functions;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Models.Operations;
using Workwear.Repository.Regulations;
using Workwear.ViewModels.Regulations;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Company.EmployeeChildren {
	public class EmployeeDutyNormsViewModel: ViewModelBase {
		private readonly EmployeeViewModel employeeViewModel;
		private readonly INavigationManager navigation;
		private readonly IProgressBarDisplayable progress;
		private readonly StockBalanceModel stockBalanceModel;
		private readonly DutyNormRepository dutyNormRepository;

		public EmployeeDutyNormsViewModel(
			EmployeeViewModel employeeViewModel,
			INavigationManager navigation,
			IProgressBarDisplayable progress,
			EmployeeIssueModel issueModel,
			StockBalanceModel stockBalanceModel,
			DutyNormRepository dutyNormRepository
			) 
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
			this.stockBalanceModel = stockBalanceModel ?? throw new ArgumentNullException(nameof(stockBalanceModel));
			this.dutyNormRepository = dutyNormRepository ?? throw new ArgumentNullException(nameof(dutyNormRepository));
		}

		#region Хелперы

		private EmployeeCard Entity => employeeViewModel.Entity;
		#region Показ
		public bool IsConfigured {get; private set; }

		public void OnShow() {
			if (IsConfigured) return;
			IsConfigured = true;
			stockBalanceModel.Warehouse = Entity.Subdivision?.Warehouse;
			Entity.FillDutyNormReceivedInfo(dutyNormRepository);
			OnPropertyChanged(nameof(ObservableDutyNormItems));
			Entity.PropertyChanged += EntityOnPropertyChanged;
		}
		#endregion
		#endregion

		#region Свойства

		public IObservableList<DutyNormItem> ObservableDutyNormItems => Entity.DutyNormItems;
		private DutyNormItem selectedDutyNormItem;
		public virtual DutyNormItem SelectedDutyNormItem {
			get => selectedDutyNormItem;
			set => SetField(ref selectedDutyNormItem, value);
		}


		#endregion
		
		#region Действия View

		public void GiveWearByDutyNorm() 
		{
			if(!employeeViewModel.Save())
				return;
			navigation.OpenViewModel<ExpenseDutyNormViewModel, IEntityUoWBuilder, EmployeeCard>(employeeViewModel, EntityUoWBuilder.ForCreate(), Entity);
		}

		public void OpenDutyNorm(DutyNormItem row) {
			var page = navigation.OpenViewModel<DutyNormViewModel, IEntityUoWBuilder, EmployeeCard>(employeeViewModel,
				EntityUoWBuilder.ForCreate(), Entity);
			page.ViewModel.SelectItem(row.Id);
		}
		#endregion

		#region Обработка изменений

		private void EntityOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(Entity.Subdivision) && Entity.Subdivision?.Warehouse != stockBalanceModel.Warehouse) {
				stockBalanceModel.Warehouse = Entity.Subdivision?.Warehouse;
				stockBalanceModel.Refresh();
			}
		}

		#endregion
		
	}
}
