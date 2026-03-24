using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.ViewModels;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.ViewModels.Stock.NomenclatureChildren {
	public class NomenclatureSizesViewModel : ViewModelBase {
		private readonly NomenclatureViewModel parent;
		private readonly INavigationManager navigation;
		
		public NomenclatureSizesViewModel(
			NomenclatureViewModel parent,
			INavigationManager navigation) {
			
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}
		
		Nomenclature Entity => parent.Entity;
		IUnitOfWork UoW => parent.UoW;
		public IObservableList<NomenclatureSizes> ObservableItems  => parent.Entity.NomenclatureSizes;
				
		#region Действия View
		
		public void Add() {
			var page = navigation.OpenViewModel<SizeWidgetViewModel,IDocItemSizeInfo, IUnitOfWork, IList<IDocItemSizeInfo>>
				(null, null, UoW,null);
			page.ViewModel.VisibleAmount = false;
		}
		
		public void Remove(NomenclatureSizes item) => Entity.NomenclatureSizes.Remove(item);
		
		#endregion
	}
}
