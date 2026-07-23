using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels;
using Workwear.Domain.ClothingService;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.ClothingService;

namespace Workwear.ViewModels.Stock.NomenclatureChildren {
	public class NomenclatureServicesViewModel : ViewModelBase {
		private readonly NomenclatureViewModel parent;
		private readonly INavigationManager navigation;
		
		public NomenclatureServicesViewModel(
			NomenclatureViewModel parent,
			INavigationManager navigation) {
			
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}
		
		Nomenclature Entity => parent.Entity;
		IUnitOfWork UoW => parent.UoW;
		public IObservableList<Service> ObservableServices  => parent.Entity.UseServices;
				
		#region Действия View
		
		public void Add() {
			var selectJournal = navigation.OpenViewModel<ServicesJournalViewModel>(parent, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += (s, e) => {
				var list = UoW.GetById<Service>(e.SelectedObjects.Select(x => x.GetId()));
				foreach(var services in list)
					Entity.AddService(services);
			};
		}
		
		public void Remove(Service service) => Entity.RemoveService(service);
		
		#endregion
	}
}
