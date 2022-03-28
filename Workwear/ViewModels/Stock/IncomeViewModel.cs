using System;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Validation;
using QS.ViewModels.Dialog;
using workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Measurements;
using workwear.Repository.Stock;
using workwear.Tools;
using workwear.Tools.Features;

namespace workwear.ViewModels.Stock
{
	public class IncomeViewModel: EntityDialogViewModelBase<Income>
	{
		private readonly FeaturesService featuresService;
		public IncomeViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			FeaturesService featuresService,
			IUserService userService,
			IValidator validator = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.featuresService = featuresService;
			if(UoW.IsNew) {
				Entity.CreatedbyUser = userService.GetCurrentUser(UoW);
			}
			Entity.ObservableItems.ListContentChanged += IncomeDoc_ObservableItems_ListContentChanged;
			CalculateTotal();
		}

		#region View
		public bool CanDelItem => Entity.ObservableItems.Count > 0;
		public bool ShowWarehouses => featuresService.Available(WorkwearFeature.Warehouses);
		public bool ButtonFillBuhDocSensitive => Entity.Items.Count > 0;

		private string sum;
		public string Sum {
			get => sum;
			set => SetField(ref sum, value);
		}
		public void AddItems() {
			var selectJournal = MainClass.MainWin.NavigationManager.
				OpenViewModel<NomenclatureJournalViewModel>(this, QS.Navigation.OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddNomenclature_OnSelectResult;
		}
		void AddNomenclature_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()))
				.ToList().ForEach(n => Entity.AddItem(n));
		}
		public void DelItems(IncomeItem item) {
			Entity.ObservableItems.Remove(item);
			OnPropertyChanged(nameof(CanDelItem));
		}
		void IncomeDoc_ObservableItems_ListContentChanged (object sender, EventArgs e)
		{
			CalculateTotal();
		}
		private void CalculateTotal()
		{
			Sum = String.Format ("Позиций в документе:{0} Количество единиц:{1} Сумма:{2:C}", 
				Entity.Items.Count,
				Entity.Items.Sum(x => x.Amount),
				Entity.Items.Sum(x => x.Total)
			);
		}
		#endregion
	}
}
