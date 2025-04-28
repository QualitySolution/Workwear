using System;
using System.Collections.Generic;
using System.Linq;
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
using QS.ViewModels.Extension;
using Workwear.Tools;
using Workwear.Tools.Sizes;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Supply;
using workwear.Journal.ViewModels.Stock;
using Workwear.Tools.Features;
using Workwear.Tools.User;
using Workwear.ViewModels.Communications;


namespace Workwear.ViewModels.Supply {
	public class ShipmentViewModel :EntityDialogViewModelBase<Shipment>, IDialogDocumentation {
		public ShipmentViewModel(
			BaseParameters baseParameters,
			CurrentUserSettings currentUserSettings,
			FeaturesService featuresService,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IInteractiveService interactive,
			IUserService userService,
			IValidator validator = null,
			UnitOfWorkProvider unitOfWorkProvider = null
		) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.currentUserSettings = currentUserSettings ?? throw new ArgumentNullException(nameof(currentUserSettings));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
            			
			if(Entity.Id == 0)
				Entity.CreatedbyUser = userService.GetCurrentUser();
////10.1
//TODO реализовать
//PreloadingDoc();
			CalculateTotal();
		}

		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("stock.html#planned-shipment");
		public string ButtonTooltip => DocHelper.GetEntityDocTooltip(Entity.GetType());
		#endregion
		
		#region Свойства ViewModel

		private readonly IInteractiveService interactive;
		private readonly CurrentUserSettings currentUserSettings;
		private readonly BaseParameters baseParameters;
		private readonly SizeService sizeService = new SizeService();
		private readonly FeaturesService featuresService;
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		
		private string total;
		public string Total {
			get => total;
			set => SetField(ref total, value);
		}
		
		private ShipmentItem selectedItem;
		[PropertyChangedAlso(nameof(CanRemoveItem))]
		public virtual ShipmentItem SelectedItem {
			get=>selectedItem;
			set=>SetField(ref selectedItem, value);
		}

		#endregion

		#region Проброс свойств документа

		public virtual string DocID {
			get {
				return Entity.Id == 0 ? "Новый" : Entity.Id.ToString();
			}
		}
		public virtual UserBase DocCreatedbyUser => Entity.CreatedbyUser;
		public virtual DateTime StartPeriod { get => Entity.StartPeriod; set => Entity.StartPeriod = value; }
		public virtual DateTime EndPeriod { get => Entity.EndPeriod; set => Entity.EndPeriod = value; }
		public virtual string DocComment { get => Entity.Comment; set => Entity.Comment = value; }
		public virtual IObservableList<ShipmentItem> Items => Entity.Items;
		
		#endregion

		#region Свойства View

		public virtual bool CanAddItem => true;
		public virtual bool CanRemoveItem => SelectedItem != null;
		public virtual bool CarEditDiffСause => Entity.Status != ShipmentStatus.New && Entity.Status != ShipmentStatus.Draft;
		public virtual bool CarEditRequested => Entity.Status == ShipmentStatus.New || Entity.Status == ShipmentStatus.Draft;
		public virtual bool CarEditOrdered => Entity.Status != ShipmentStatus.Ordered || Entity.Status != ShipmentStatus.Received;
		public virtual bool CanSandEmail => featuresService.Available(WorkwearFeature.Communications);
		
		public virtual IList<Size> GetSizeVariants(ShipmentItem item) {
			return sizeService.GetSize(UoW, item.WearSizeType, onlyUseInNomenclature: true).ToList();
		}
		
		public virtual IList<Size> GetHeightVariants(ShipmentItem item) {
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
		
		public void DeleteItem(ShipmentItem item) {
			Entity.RemoveItem(item); 
			OnPropertyChanged(nameof(CanRemoveItem));
			CalculateTotal();
		}
		private void CalculateTotal() {
			Total = $"Позиций в документе: {Entity.Items.Count}  " +
			        $"Количество единиц: {Entity.Items.Sum(x => x.Requested)} " +
			        $"Сумма: {Entity.Items.Sum(x => x.Requested * x.Cost)}{baseParameters.UsedCurrency}";
		}

		public void SendMessegeForBuyer() {
			var dialoog = NavigationManager.OpenViewModel<SendEmailViewModel>(this);
			dialoog.ViewModel.EmailAddres = currentUserSettings.Settings.BuyerEmail;
			dialoog.ViewModel.Topic = "Новая планируемая поставка Сппецодежды.";
			dialoog.ViewModel.Messege = "Добрый день!\nВ программе QS создана новая заявка на закупку. Просим принять в работу.";
			dialoog.ViewModel.Title = "Оповестить закупку";
			
			dialoog.ViewModel.ShowSaveAddres = true;
			dialoog.ViewModel.SaveAdressFunc = adress => {
				currentUserSettings.Settings.BuyerEmail = adress;
				currentUserSettings.SaveSettings();
			};
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
				                    + $", общим количеством {duplicate.Sum(x=>x.Requested)} \n";
			}
			if(!String.IsNullOrEmpty(duplicateMessage) && !interactive.Question($"В документе есть повторяющиеся позиции:\n{duplicateMessage}\n Сохранить документ?"))
				return false;

			Entity.FullOrdered = Items.All(i => i.Ordered >= i.Requested);
			
			if(Entity.Id == 0) 
				Entity.CreationDate = DateTime.Today;
			UoWGeneric.Save ();
			logger.Info ("Документ сохранён.");
			return true;
		}

		#endregion
	}
}
