using System;
using System.Linq;
using Gamma.Widgets;
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
using Workwear.Tools.Features;

namespace Workwear.ViewModels.ClothingService {
	public class ServiceViewModel : EntityDialogViewModelBase<Service> {
		public ServiceViewModel(
			FeaturesService featuresService,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator = null) 
			: base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
		}
		
		private readonly INavigationManager navigation;
		private readonly FeaturesService featuresService;

		public bool ShowAlternativeName => featuresService.Available(WorkwearFeature.ReportServiceServiced);
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
		
		public object SelectState {
			get => (object)Entity.WithState ?? SpecialComboState.Not;
			set {
				switch(value) {
					case (SpecialComboState.Not): Entity.WithState = null; break;
					default: Entity.WithState = (ClaimState?)value; break;
				}
				OnPropertyChanged();
			}
		}
		
		public override bool Save() {

			if(Entity.Code == null)
				BarcodeService.SetClothingServiceCode(UoW, Entity);
			
			return base.Save();
		}

		public void RemoveNomenclature(Nomenclature nomenclature) => Entity.RemoveNomenclature(nomenclature);
	}
}
