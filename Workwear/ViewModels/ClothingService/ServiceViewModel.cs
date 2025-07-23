using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Barcodes;

namespace Workwear.ViewModels.ClothingService {
	public class ServiceViewModel : EntityDialogViewModelBase<Service> {
		public ServiceViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null) 
			: base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}
		
		private readonly INavigationManager navigation;

		public IObservableList<Nomenclature> ObservableNomenclatures  => Entity.Nomenclatures;
		public void AddNomenclature() {
			var selectJournal = navigation.OpenViewModel<NomenclatureJournalViewModel>(this, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += (s, e) => {
				var list = UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()));
				foreach(var n in list)
					Entity.AddNomenclature(n);
			};
		}

		public override bool Save() {

			if(Entity.Code == null)
				BarcodeService.SetClothingServiceCode(UoW, Entity);
			
			return base.Save();
		}

		public void RemoveNomenclature(Nomenclature nomenclature) => Entity.RemoveNomenclature(nomenclature);
	}
}
