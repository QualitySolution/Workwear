using System;
using System.Collections.Generic;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Workwear.Domain.Company;
using workwear.Representations.Organization;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;

namespace Workwear.ViewModels.Company.EmployeeChildren
{
	public class EmployeeListedItemsViewModel : ViewModelBase
	{
		private readonly IInteractiveService interactive;
		private string answer;
		public EmployeeListedItemsViewModel(EmployeeViewModel employeeViewModel, ITdiCompatibilityNavigation navigation, FeaturesService featuresService, IInteractiveService interactive)
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.featuresService = featuresService;
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
		}

		#region Хелперы

		private IUnitOfWork UoW => employeeViewModel.UoW;
		private EmployeeCard Entity => employeeViewModel.Entity;

		#endregion

		#region Показ
		private bool isConfigured = false;
		private readonly EmployeeViewModel employeeViewModel;
		private readonly FeaturesService featuresService;
		private readonly ITdiCompatibilityNavigation navigation;

		public void OnShow()
		{
			if(!isConfigured) {
				isConfigured = true;
				EmployeeBalanceVM = new EmployeeBalanceVM(UoW) {
					Employee = Entity
				};
			}
		}

		public void UpdateList()
		{
			EmployeeBalanceVM.UpdateNodes();
		}

		#endregion

		#region Свойства

		private EmployeeBalanceVM employeeBalanceVM;
		public EmployeeBalanceVM EmployeeBalanceVM { get => employeeBalanceVM; private set => SetField(ref employeeBalanceVM, value); }

		public bool SensetiveButtonGiveWear => !employeeViewModel.UoW.IsNew;
		public bool SensetiveButtonReturn => !employeeViewModel.UoW.IsNew;
		public bool SensetiveButtonWriteoff => !employeeViewModel.UoW.IsNew;
		public bool SensetiveButtonInspecton => !employeeViewModel.UoW.IsNew;
		public bool VisibleButtonInspecton => featuresService.Available(WorkwearFeature.Inspection);

		#endregion

		#region Действия View

		public void GiveWear()
		{
			if(!employeeViewModel.Save())
				return;
			if(Entity?.DismissDate != null) {
				answer = interactive.Question(new[] { "Выдать всё", "Пустой", "Отмена" }, $"У сотрудника {Entity.FullName} " +
				                                                                          $"указана дата увольнения: {Entity.DismissDate?.ToShortDateString()}. Выдать?",
					"Предупреждение о наличии даты увольнения у выбранного сотрудника");
			}
			if(answer != "Отмена")
				navigation.OpenViewModel<ExpenseEmployeeViewModel, IEntityUoWBuilder, EmployeeCard, string>(employeeViewModel, EntityUoWBuilder.ForCreate(), Entity, answer);
		}

		public void ReturnWear(EmployeeBalanceVMNode node)
		{
			var  page = navigation.OpenViewModel<ReturnViewModel, IEntityUoWBuilder, EmployeeCard>
				(employeeViewModel,EntityUoWBuilder.ForCreate(),Entity);
			if(node != null)
				page.ViewModel.AddFromDictionary(new Dictionary<int, int>() {{node.Id, node.Balance}});
		}

		public void WriteOffWear()
		{
			navigation.OpenViewModel<WriteOffViewModel, IEntityUoWBuilder, EmployeeCard>(employeeViewModel, EntityUoWBuilder.ForCreate(), Entity);
		}

		public void InspectionWear() {
			navigation.OpenViewModel<InspectionViewModel, IEntityUoWBuilder, EmployeeCard>(employeeViewModel, EntityUoWBuilder.ForCreate(), Entity);
		}
		#endregion

	}
}
