using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Utils;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Report.ViewModels;
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
using Workwear.Models.Analytics.WarehouseForecasting;
using Workwear.Tools.Features;
using Workwear.Tools.User;
using Workwear.ViewModels.Analytics;
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
			UnitOfWorkProvider unitOfWorkProvider = null,
			List<WarehouseForecastingItem> forecastingItems = null,
			ShipmentCreateType eItemEnum = default
		) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.baseParameters = baseParameters ?? throw new ArgumentNullException(nameof(baseParameters));
			this.currentUserSettings = currentUserSettings ?? throw new ArgumentNullException(nameof(currentUserSettings));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
            			
			if(Entity.Id == 0)
				Entity.CreatedbyUser = userService.GetCurrentUser();

			if(forecastingItems != null) 
				AddFromForecasting(forecastingItems, eItemEnum);
			
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

		public void SendMessageForBuyer() {
			var dialog = NavigationManager.OpenViewModel<SendEmailViewModel>(this);
			dialog.ViewModel.EmailAddress = currentUserSettings.Settings.BuyerEmail;
			dialog.ViewModel.Topic = "Новая планируемая поставка Спецодежды.";
			dialog.ViewModel.Message = "Добрый день!\nВ программе QS создана новая заявка на закупку. Просим принять в работу.";
			dialog.ViewModel.Title = "Оповестить закупку";
			
			dialog.ViewModel.ShowSaveAddress = true;
			dialog.ViewModel.SaveAddressFunc = address => {
				currentUserSettings.Settings.BuyerEmail = address;
				currentUserSettings.SaveSettings();
			};
		}
		
		public void AddFromForecasting(List<WarehouseForecastingItem> forecastingItems, ShipmentCreateType eItemEnum) {

			var nomIds = forecastingItems
				.Where( i => i.Nomenclature != null)
				.Select(i => i.Nomenclature)
				.Select(n => n.Id).ToList();
			var sizeIds = forecastingItems
				.Where( i => i.Size != null)
				.Select(i => i.Size)
				.Select(n => n.Id).ToList();
			sizeIds.AddRange(forecastingItems
				.Where( i => i.Height != null)
				.Select(i => i.Height)
				.Select(n => n.Id).ToList());
			
			UoW.Session.QueryOver<Nomenclature>()
				.Where(x => x.Id.IsIn(nomIds))
				.Fetch(SelectMode.Fetch, n => n.Type.Units)
				.Future();
			UoW.Session.QueryOver<Size>()
				.Where(x => x.Id.IsIn(sizeIds))
				.Future();
			
			foreach(var fitem in forecastingItems.Where( i => i.Nomenclature != null)) {
				if(eItemEnum == ShipmentCreateType.WithDebt && fitem.WithDebt < 0 ||
				   eItemEnum == ShipmentCreateType.WithoutDebt && fitem.WithoutDebt < 0)
					Entity.AddItem(
						UoW.GetInSession(fitem.Nomenclature),
						UoW.GetInSession(fitem.Size),
						UoW.GetInSession(fitem.Height),
						(eItemEnum == ShipmentCreateType.WithDebt ? fitem.WithDebt : fitem.WithoutDebt) * -1,
						fitem.Nomenclature.SaleCost ?? 0 //Возможно стоит подвязаться на переключатель типа стоимости в прогнозе
						);
			}
		}
		#endregion

		#region Валидация, сохранение и печать

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
		public void Print() 
		{
			if(UoW.HasChanges && !interactive.Question("Перед печатью документ будет сохранён. Продолжить?"))
				return;
			if (!Save())
				return;
			
			var reportInfo = new ReportInfo {
				Title = Entity.Title,
				Identifier = "Documents.ShipmentSheet",
				Parameters = new Dictionary<string, object> {
					{ "shipment_id",  Entity.Id },
					{ "printPromo", featuresService.Available(WorkwearFeature.PrintPromo)},
				}
			};
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(this, reportInfo);
		}

		#endregion
	}
}
