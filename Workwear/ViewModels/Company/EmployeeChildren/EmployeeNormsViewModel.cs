using System;
using System.Data.Bindings.Collections.Generic;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Regulations;
using Workwear.ViewModels.Regulations;

namespace Workwear.ViewModels.Company.EmployeeChildren
{
	public class EmployeeNormsViewModel : ViewModelBase
	{
		private readonly EmployeeViewModel employeeViewModel;
		private readonly INavigationManager navigation;
		private readonly IInteractiveService interactive;

		public EmployeeNormsViewModel(EmployeeViewModel employeeViewModel, INavigationManager navigation, IInteractiveService interactive)
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));

			Entity.PropertyChanged += Entity_PropertyChanged;
		}

		#region Хелперы

		private IUnitOfWork UoW => employeeViewModel.UoW;
		private EmployeeCard Entity => employeeViewModel.Entity;

		public GenericObservableList<Norm> ObservableUsedNorms => Entity.ObservableUsedNorms;

		#endregion

		#region Публичные
		private bool isConfigured = false;

		public void OnShow()
		{
			if(!isConfigured) {
				isConfigured = true;
				OnPropertyChanged(nameof(ObservableUsedNorms));
			}
		}

		#endregion

		#region Sensetive

		public bool SensetiveNormFromPost => Entity.Post != null;

		#endregion

		#region Действия View

		public void AddNorm()
		{
			if(Entity.Id == 0) {
				if(interactive.Question("Перед добавлением нормы необходимо сохранить сотрудника. Сохраняем?")) {
					if(!employeeViewModel.Save()) //Здесь если не сохраним нового сотрудника при установки нормы скорей всего упадем.
						return;
				}
				else 
					return;
			}
			
			var selectPage = navigation.OpenViewModel<NormJournalViewModel>(
				employeeViewModel,
				OpenPageOptions.AsSlave
			);
			selectPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectPage.ViewModel.OnSelectResult += NormJournal_OnSelectResult;
		}

		void NormJournal_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var norm in e.SelectedObjects) {
				Entity.AddUsedNorm(UoW.GetById<Norm>(norm.GetId()));
			}
		}

		public void RemoveNorm(Norm norm)
		{
			Entity.RemoveUsedNorm(norm);
		}

		public void NormFromPost()
		{
			if(Entity.Id == 0 && !employeeViewModel.Save()) { //Здесь если не сохраним нового сотрудника при установки нормы скорей всего упадем.
				interactive.ShowMessage(ImportanceLevel.Error, "Норма не будет установлена, так как не все данные сотрудника заполнены корректно.");
				return;
			}
			Entity.NormFromPost(UoW, employeeViewModel.NormRepository);
		}

		public void OpenNorm(Norm norm)
		{
			navigation.OpenViewModel<NormViewModel, IEntityUoWBuilder>(employeeViewModel, EntityUoWBuilder.ForOpen(norm.Id));
		}

		#endregion

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Post))
				OnPropertyChanged(nameof(SensetiveNormFromPost));
		}
	}
}
