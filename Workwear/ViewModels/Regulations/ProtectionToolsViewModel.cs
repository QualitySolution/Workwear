using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using QSOrmProject;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Regulations;

namespace workwear.ViewModels.Regulations
{
	public class ProtectionToolsViewModel : EntityDialogViewModelBase<ProtectionTools>
	{
		ITdiCompatibilityNavigation tdiNavigationManager;

		public ProtectionToolsViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ITdiCompatibilityNavigation navigation, IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			tdiNavigationManager = navigation;
		}

		#region Действия View
		#region Аналоги
		public void AddAnalog()
		{
			var page = NavigationManager.OpenViewModel<ProtectionToolsJournalViewModel>(this, OpenPageOptions.AsSlave);
			page.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += Analog_OnSelectResult;
		}

		void Analog_OnSelectResult(object sender, QS.Project.Journal.JournalSelectedEventArgs e)
		{
			foreach(var toolsNode in e.SelectedObjects) {
				var tools = UoW.GetById<ProtectionTools>(toolsNode.GetId());
				Entity.AddAnalog(tools);
			}
		}

		public void RemoveAnalog(ProtectionTools[] tools)
		{
			foreach(var item in tools) {
				Entity.RemoveAnalog(item);
			}
		}
		#endregion
		#region Номеклатуры
		public void AddNomeclature()
		{
			var selectPage = tdiNavigationManager.OpenTdiTab<OrmReference, Type>(this, typeof(Nomenclature), OpenPageOptions.AsSlave);
			var tab = selectPage.TdiTab as OrmReference;
			tab.Mode = OrmReferenceMode.MultiSelect;
			tab.ObjectSelected += Nomenclature_ObjectSelected;
		}

		void Nomenclature_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var nomenclatures = UoW.GetById<Nomenclature>(e.Subjects.Select(x => x.GetId()));
			foreach(var nomenclature in nomenclatures) {
				Entity.AddNomeclature(nomenclature);
			}
		}

		public void RemoveNomeclature(Nomenclature[] tools)
		{
			foreach(var item in tools) {
				Entity.RemoveNomeclature(item);
			}
		}
		#endregion
		#endregion
	}
}
