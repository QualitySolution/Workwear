using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Sizes;
using workwear.Journal.ViewModels.Stock;
using Workwear.Measurements;

namespace workwear.ViewModels.Stock
{
	public class SizeViewModel: EntityDialogViewModelBase<Size>
	{
		public SizeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			SizeType sizeType = null,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			Validations.Clear();
			Validations.Add(new ValidationRequest(Entity, 
				new ValidationContext(Entity, new Dictionary<object, object> {{nameof(IUnitOfWork), UoW} })));
			if (UoW.IsNew) {
				Entity.SizeType = sizeType;
				IsNew = true;
			}
			else {
				if (Entity.Id <= SizeService.MaxStandartSizeId) IsStandart = true;
			}
		}
		
		public bool IsStandart { get; }
		public bool IsNew {get;}
		public bool CanEdit => IsNew || !IsStandart;

		public void AddAnalog() {
			var selectJournal = MainClass.MainWin.NavigationManager.
				OpenViewModel<SizeJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.SizeType = Entity.SizeType;
			selectJournal.ViewModel.Filter.Sensitive = false;
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += SelectFromStock_OnSelectResult;
		}

		private void SelectFromStock_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			var selectVM = sender as SizeJournalViewModel;
			foreach (var node in e.GetSelectedObjects<SizeJournalNode>()) {
				var analog = UoW.GetById<Size>(node.Id);
				Entity.ObservableSuitableSizes.Add(analog);
			}
		}
		public void RemoveAnalog(Size analog) => 
			Entity.ObservableSuitableSizes.Remove(analog);
	}
}
