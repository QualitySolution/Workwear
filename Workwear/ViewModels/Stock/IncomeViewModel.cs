using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Utilities;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using workwear.Journal.ViewModels.Stock;
using Workwear.Repository.Stock;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Stock.Widgets;

namespace Workwear.ViewModels.Stock {
	public class IncomeViewModel  : EntityDialogViewModelBase<Income> {
		public IncomeViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IInteractiveService interactive,
			ILifetimeScope autofacScope,
			StockRepository stockRepository,
			BaseParameters baseParameters,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null
			) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider)
		{
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			featuresService = autofacScope.Resolve<FeaturesService>();
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			
			if(featuresService.Available(WorkwearFeature.Owners))
				owners = UoW.GetAll<Owner>().ToList();
			
			if(featuresService.Available(WorkwearFeature.Warehouses) && Entity.Warehouse == null)
				Entity.Warehouse = stockRepository.GetDefaultWarehouse(UoW, featuresService, autofacScope.Resolve<IUserService>().CurrentUserId);
			
			
			var entryBuilder = new CommonEEVMBuilderFactory<Income>(this, Entity, UoW, navigation, autofacScope);
			
			WarehouseEntryViewModel = entryBuilder.ForProperty(x => x.Warehouse)
				.UseViewModelJournalAndAutocompleter<WarehouseJournalViewModel>()
				.UseViewModelDialog<WarehouseViewModel>()
				.Finish();
			
			CalculateTotal();
		}

		#region Свойства VikewModel
		private readonly IInteractiveService interactive;
		private readonly BaseParameters baseParameters;
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		
		private string total;
		public string Total {
			get => total;
			set => SetField(ref total, value);
		}
		#endregion
		
		#region Проброс свойств документа

		public virtual int DocID => Entity.Id;
		public virtual string DocTitle => Entity.Title;
		public virtual UserBase DocCreatedbyUser => Entity.CreatedbyUser;
		public virtual string DocComment { get => Entity.Comment; set => Entity.Comment = value;}
		public virtual string NumberTN { get => Entity.Number; set => Entity.Number = value;}
		public virtual DateTime DocDate { get => Entity.Date;set => Entity.Date = value;}
		public virtual IObservableList<IncomeItem> Items => Entity.Items;

		#endregion

		#region Свойства ViewModel
		private readonly FeaturesService featuresService;
		private readonly SizeService sizeService = new SizeService();
		public readonly EntityEntryViewModel<Warehouse> WarehouseEntryViewModel;
		
		private List<Owner> owners = new List<Owner>();
		public List<Owner> Owners => owners;

		
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
		public virtual bool CanAddSize => SelectedItem != null && (SelectedItem.WearSizeType != null || SelectedItem.HeightType != null);
		public virtual bool OwnersVisible => featuresService.Available(WorkwearFeature.Owners);
		public virtual bool WarehouseVisible => featuresService.Available(WorkwearFeature.Warehouses);
		public virtual bool ReadInFileVisible  => featuresService.Available(WorkwearFeature.Exchange1C);
		public virtual bool CanReadFile => false;
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
			selectJournal.ViewModel.OnSelectResult += AddItem_OnSelectResult;
		}

		private void AddItem_OnSelectResult(object sender, JournalSelectedEventArgs e) {
			UoW.GetById<Nomenclature>(e.SelectedObjects.Select(x => x.GetId()))
				.ToList().ForEach(n => Entity.AddItem(n, interactive));
			CalculateTotal();
		}
		public void DeleteItem(IncomeItem item) {
			Entity.RemoveItem(item); 
			OnPropertyChanged(nameof(CanRemoveItem));
			OnPropertyChanged(nameof(CanAddSize));
			CalculateTotal();
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

			if(item.WearSize == null) {
				Entity.RemoveItem(item);
				OnPropertyChanged(nameof(CanRemoveItem));
				OnPropertyChanged(nameof(CanAddSize));
			}

			CalculateTotal();
		}
		
		private void CalculateTotal() {
			Total = $"Позиций в документе: {Entity.Items.Count}  " +
			        $"Количество единиц: {Entity.Items.Sum(x => x.Amount)} " +
			        $"Сумма: {Entity.Items.Sum(x => x.Amount * x.Cost)}{baseParameters.UsedCurrency}";
		}
		#endregion

		#region Валидация и сохранение

		public override bool Save() {
			logger.Info ("Запись документа...");
			
			logger.Info("Валидация...");
			if(!Validate()) {
				logger.Warn("Валидация не пройдена, сохранение отменено.");
				return false;
			} else 
				logger.Info("Валидация пройдена.");
			
			logger.Info ("Проверка на дубли");
			string duplicateMessage = "";
			foreach(var duplicate in Entity.Items.GroupBy(x => x.StockPosition).Where(x => x.Count() > 1)) {
				duplicateMessage += $"- {duplicate.First().StockPosition.Title} указано " +
				                    $"{NumberToTextRus.FormatCase(duplicate.Count(), "{0} раз", "{0} раза", "{0} раз")}" 
				                    + $", общим количеством {duplicate.Sum(x=>x.Amount)} \n";
			}
			if(!String.IsNullOrEmpty(duplicateMessage) && !interactive.Question($"В документе есть повторяющиеся складские позиции:\n{duplicateMessage}\n Сохранить документ?"))
				return false;

			logger.Info ("Обновлление складских операций");
			Entity.UpdateOperations(UoW);
			if(Entity.Id == 0)
				Entity.CreationDate = DateTime.Now;
			UoWGeneric.Save ();

			logger.Info ("Документ сохранён.");
			return true;
		}
		
		#endregion
	}
}
