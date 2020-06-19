using System;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.ViewModels;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Regulations;
using workwear.ViewModels.Regulations;

namespace workwear.ViewModels.Company.EmployeeChilds
{
	public class EmployeeNormsViewModel : ViewModelBase
	{
		private readonly EmployeeViewModel employeeViewModel;
		private readonly INavigationManager navigation;

		public EmployeeNormsViewModel(EmployeeViewModel employeeViewModel, INavigationManager navigation)
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));

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
				Entity.AddUsedNorm(UoW.GetById<Norm>(e.GetId()));
			}
		}

		public void RemoveNorm(Norm norm)
		{
			Entity.RemoveUsedNorm(norm);
		}

		public void NormFromPost()
		{
			var norms = Repository.NormRepository.GetNormForPost(UoW, Entity.Post);
			foreach(var norm in norms)
				Entity.AddUsedNorm(norm);
		}

		public void RefreshWorkwearItems()
		{
			Entity.UpdateWorkwearItems();
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
