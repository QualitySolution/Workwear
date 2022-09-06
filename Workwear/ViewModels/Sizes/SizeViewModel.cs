using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Sizes;
using workwear.Journal.ViewModels.Stock;
using Workwear.Measurements;

namespace workwear.ViewModels.Sizes
{
	public class SizeViewModel: EntityDialogViewModelBase<Size>
	{
		public SizeService SizeService { get; }
		public SizeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			SizeService sizeService,
			SizeType sizeType = null,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			SizeService = sizeService;
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
			
			Entity.ObservableSuitableSizes.ListContentChanged += ObservableSuitableSizesOnListContentChanged;
		}

		private void ObservableSuitableSizesOnListContentChanged(object sender, EventArgs e) => OnPropertyChanged(nameof(AllSuitable));
		public IList<Size> AllSuitable => Entity.SuitableSizes.Union(Entity.SizesWhereIsThisSizeAsSuitable).ToList();

		public bool IsStandart { get; }
		public bool IsNew {get;}
		public bool CanEdit => IsNew || !IsStandart;

		public void AddAnalog() {
			var selectJournal = MainClass.MainWin.NavigationManager.
				OpenViewModel<SizeJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.SelectedSizeType = Entity.SizeType;
			selectJournal.ViewModel.Filter.SensitiveSizeType = false;
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += SelectFromStock_OnSelectResult;
		}

		private void SelectFromStock_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			var selects = e.GetSelectedObjects<SizeJournalNode>();
			var analogs = UoW.GetById<Size>(selects.Select(x => x.Id));
			foreach (var analog in analogs) {
				Entity.ObservableSuitableSizes.Add(analog);
			}
		}
		public void RemoveAnalog(Size analog) {
			if(Entity.ObservableSuitableSizes.Contains(analog))
				Entity.ObservableSuitableSizes.Remove(analog);
			else {
				Entity.SizesWhereIsThisSizeAsSuitable.Remove(analog);
				ObservableSuitableSizesOnListContentChanged(this, null);
			}
		}
	}
}
