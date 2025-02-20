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
using QS.Services;
using QS.Utilities;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools;
using Workwear.Tools.Features;
using Workwear.Tools.Sizes;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using workwear.Journal.ViewModels.Stock;

namespace Workwear.ViewModels.Stock {
	public class ProcurementViewModel :EntityDialogViewModelBase<Procurement> {
		public ProcurementViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IInteractiveService interactive,
			ILifetimeScope autofacScope,
			BaseParameters baseParameters,
			IUserService userService,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null
		) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			featuresService = autofacScope.Resolve<FeaturesService>();
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			
			if(Entity.Id == 0)
				Entity.CreatedbyUser = userService.GetCurrentUser();
			
			
		}

		#region Свойства ViewModel

		private readonly IInteractiveService interactive;
		private readonly BaseParameters baseParameters;
		private readonly FeaturesService featuresService;
		private readonly SizeService sizeService = new SizeService();
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		
		private string total;
		public string Total {
			get => total;
			set => SetField(ref total, value);
		}
		
		private ProcurementItem selectedItem;
		public virtual ProcurementItem SelectedItem {
			get=>selectedItem;
			set=>SetField(ref selectedItem, value);
		}

		#endregion

		#region Проброс свойств документа

		public virtual int DocID => Entity.Id;
		public virtual string DocTitle => Entity.Title;
		public virtual UserBase DocCreatedbyUser => Entity.CreatedbyUser;
		public virtual DateTime DocDate { get => Entity.CreationDate;set => Entity.CreationDate = value;}
		public virtual string DocComment { get => Entity.Comment; set => Entity.Comment = value;}
		public virtual IObservableList<ProcurementItem> Items => Entity.Items;
		
		#endregion

		#region Свойства View

		public virtual bool CanAddItem => true;
		public virtual bool CanRemoveItem => SelectedItem != null;
		
		public virtual IList<Size> GetSizeVariants(ProcurementItem item) {
			return sizeService.GetSize(UoW, item.WearSizeType, onlyUseInNomenclature: true).ToList();
		}
		
		public virtual IList<Size> GetHeightVariants(ProcurementItem item) {
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
		
		public void DeleteItem(ProcurementItem item) {
			Entity.RemoveItem(item); 
			OnPropertyChanged(nameof(CanRemoveItem));
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
			if(!String.IsNullOrEmpty(duplicateMessage) && !interactive.Question($"В документе есть повторяющиеся позиции:\n{duplicateMessage}\n Сохранить документ?"))
				return false;
			
			
			logger.Info ("Документ сохранён.");
			return true;
		}

		#endregion
	}
}
