using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.ViewModels;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.ViewModels.Stock.NomenclatureChildren {
	public class NomenclatureSizesViewModel : ViewModelBase {
		private readonly NomenclatureViewModel parent;
		private readonly INavigationManager navigation;
		public readonly SizeService SizeService;
		
		
		public NomenclatureSizesViewModel(
			NomenclatureViewModel parent,
			SizeService sizeService,
			INavigationManager navigation) {
			
			this.SizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}
		
		Nomenclature Entity => parent.Entity;
		public IUnitOfWork UoW => parent.UoW;
		public IObservableList<NomenclatureSizes> Items  => parent.Entity.NomenclatureSizes;
				
		#region Действия View
		
		public void Add() {
			var page = navigation.OpenViewModel<SizeWidgetViewModel,IDocItemSizeInfo, IUnitOfWork, IList<IDocItemSizeInfo>>
				(null, new NomenclatureSizes(){Nomenclature = parent.Entity}, UoW, Items.Cast<IDocItemSizeInfo>().ToList());
//// Возможно, имеет смысл передавать через Autofac			
			page.ViewModel.VisibleAmount = false;
			page.ViewModel.AddedSizes += (i, args) => AddResultSizes(args);
		}
		
		private void AddResultSizes(AddedSizesEventArgs args) {
			foreach(var s in args.SizesWithAmount) {
				if(!Items.Any(x => DomainHelper.EqualDomainObjects(x.Height, args.Height)
				                  && DomainHelper.EqualDomainObjects(x.Size, s.Size))) {
					Items.Add(new NomenclatureSizes(){Nomenclature = parent.Entity, Height = args.Height, Size = s.Size});
				}
			}
		}
		
		public void Remove(NomenclatureSizes item) => Entity.NomenclatureSizes.Remove(item);
		
		#endregion
	}
}
