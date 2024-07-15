using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.ViewModels.Stock {
	public class IncomeViewModel  : EntityDialogViewModelBase<Income> {
		
		private readonly IInteractiveMessage interactive;
		public IncomeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IInteractiveMessage interactive,
			ILifetimeScope autofacScope,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
			featuresService = autofacScope.Resolve<FeaturesService>();
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			
			if(featuresService.Available(WorkwearFeature.Owners))
				Owners = UoW.GetAll<Owner>().ToList();
			if(featuresService.Available(WorkwearFeature.Warehouses))
				Warhouses = UoW.GetAll<Warehouse>().ToList();
			
			var entryBuilder = new CommonEEVMBuilderFactory<Income>(this, Entity, UoW, navigation, autofacScope);
			
			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
		}
		
		#region Проброс свойств документа

		public virtual int DocID => Entity.Id;
		public virtual string DocTitle => Entity.Title;
		public virtual string DocComment => Entity.Comment;
		public virtual UserBase DocCreatedbyUser => Entity.CreatedbyUser;
		public virtual DateTime DocDate => Entity.Date;
		public virtual Warehouse Warehouse => Entity.Warehouse;
		public virtual string NumberTN => Entity.Number;
		public virtual IObservableList<IncomeItem> Items => Entity.Items;

		#endregion

		#region Свойства ViewModel
		private readonly FeaturesService featuresService;
		private readonly SizeService sizeService = new SizeService();
		public readonly EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		
		//public IList<Owner> Owners = QS.DomainModel.UoW.UnitOfWorkFactory.CreateWithoutRoot().GetAll<Owner>().ToList();
		//private List<Owner> owners = new List<Owner>();
		public List<Owner> Owners {get;}
		public List<Warehouse> Warhouses {get;}
		
		private bool autoDocNumber = true;
		[PropertyChangedAlso(nameof(DocNumberText))]
		[PropertyChangedAlso(nameof(SensitiveDocNumber))]
		public virtual bool AutoDocNumber {
			get => autoDocNumber;
			set => SetField(ref autoDocNumber, value);
		}

		public virtual string DocNumberText {
			get => AutoDocNumber ? (Entity.Id != 0 ? Entity.Id.ToString() : "авто" ) : Entity.DocNumber;
			set => Entity.DocNumber = (AutoDocNumber || value == "авто") ? null : value;
		}

		#endregion

		#region Свойства Viewmodel

		private IncomeItem selectedItem;
		[PropertyChangedAlso(nameof(CanRemoveItem))]
		[PropertyChangedAlso(nameof(CanAddSize))]
		public virtual IncomeItem SelectedItem {
			get => selectedItem;
			set => SetField(ref selectedItem, value);
		}

		#endregion
		
		
		#region Свойства для View

		public virtual bool SensitiveDocNumber => !AutoDocNumber;
		public virtual bool CanAddItem => true;
		public virtual bool CanRemoveItem => SelectedItem != null;
		public virtual bool CanAddSize => SelectedItem != null && SelectedItem.WearSizeType != null && SelectedItem.HeightType != null;
		public virtual bool OwnersVisible => featuresService.Available(WorkwearFeature.Owners);
		public virtual bool ReadInFileVisible => featuresService.Available(WorkwearFeature.Warehouses);
		public virtual bool CanReadFile => false;
		public virtual bool WarehouseVisible => featuresService.Available(WorkwearFeature.Exchange1C);

		public virtual IList<Size> GetSizeVariants(IncomeItem item) {
			return sizeService.GetSize(UoW, item.WearSizeType, onlyUseInNomenclature: true).ToList();
		}
		
		public virtual IList<Size> GetHeightVariants(IncomeItem item) {
			return sizeService.GetSize(UoW, item.HeightType, onlyUseInNomenclature: true).ToList();
		}
		#endregion

		#region Методы
		public void AddItem() {
			var selectJournal = NavigationManager
				.OpenViewModel<NomenclatureJournalViewModel>( this,OpenPageOptions.AsSlave);
			selectJournal.ViewModel.Filter.ShowArchival = false;
			selectJournal.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += AddNomenclature_OnSelectResult;
		}

		private void AddNomenclature_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()))
				.ToList().ForEach(n => Entity.AddItem(n, interactive));
			//CalculateTotal();
		}
		public void AddSize(IncomeItem item) {
			if(item.Nomenclature == null)
				return;
			var existItems = Entity.Items
				.Where(i => i.Nomenclature.IsSame(item.Nomenclature) && i.Owner == item.Owner)
				.Cast<IDocItemSizeInfo>().ToList();
			var selectJournal = NavigationManager
				.OpenViewModel<SizeWidgetViewModel, IDocItemSizeInfo, IUnitOfWork, IList<IDocItemSizeInfo>>
					(null, item, UoW, existItems);
			selectJournal.ViewModel.AddedSizes += (s, e) => SelectWearSize_SizeSelected(e, item);
		}
		
		private void SelectWearSize_SizeSelected(AddedSizesEventArgs e, IncomeItem item) {
			foreach (var i in e.SizesWithAmount.ToList()) {
				var exist = Entity.FindItem(item.Nomenclature, i.Size, e.Height, item.Owner);
				if(exist != null)
					exist.Amount = i.Amount;
				else
					Entity.AddItem(item.Nomenclature,  i.Size, e.Height, i.Amount, item.Certificate, item.Cost, item.Owner);
			}
			if(item.WearSize == null)
				Entity.RemoveItem(item);
			//CalculateTotal();
		}
		#endregion
	}
}
