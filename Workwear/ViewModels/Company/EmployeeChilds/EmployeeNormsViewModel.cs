using System;
using System.Data.Bindings.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using workwear.Dialogs.Regulations;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.ViewModel;

namespace workwear.ViewModels.Company.EmployeeChilds
{
	public class EmployeeNormsViewModel : ViewModelBase
	{
		private readonly EmployeeViewModel employeeViewModel;
		private readonly ITdiCompatibilityNavigation navigation;

		public EmployeeNormsViewModel(EmployeeViewModel employeeViewModel, ITdiCompatibilityNavigation navigation)
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
			var selectPage = navigation.OpenTdiTab<ReferenceRepresentation>(
				employeeViewModel,
				OpenPageOptions.AsSlave,
				c => c.RegisterType<NormVM>().As<IRepresentationModel>()
			);
			var refWin = selectPage.TdiTab as ReferenceRepresentation;
			refWin.Mode = OrmReferenceMode.Select;
			refWin.ObjectSelected += RefWin_ObjectSelected;
		}

		void RefWin_ObjectSelected(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			Entity.AddUsedNorm(UoW.GetById<Norm>(e.ObjectId));
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
			navigation.OpenTdiTab<NormDlg, Norm>(employeeViewModel, norm);
		}

		#endregion

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Post))
				OnPropertyChanged(nameof(SensetiveNormFromPost));
		}

	}
}
