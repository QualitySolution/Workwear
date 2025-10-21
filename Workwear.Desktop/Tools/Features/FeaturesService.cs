using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.Utilities;
using QS.Cloud.Client;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Project.DB;
using QS.Project.Versioning.Product;
using QS.Serial;
using QS.Serial.Encoding;

namespace Workwear.Tools.Features
{
	public class FeaturesService : IProductService
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static ProductEdition[] SupportEditions = new[] {
			new ProductEdition(0, "Демонстрационная"),
			new ProductEdition(1, "Однопользовательская"),
			new ProductEdition(2, "Профессиональная"),
			new ProductEdition(3, "Предприятие"),
			new ProductEdition(4, "Спецаутсорсинг")
		};
		private readonly SerialNumberEncoder serialNumberEncoder;
		private readonly CloudClientService cloudClientService;
		private readonly IInteractiveMessage interactive;
		private readonly IErrorReporter errorReporter;
		private readonly ILifetimeScope autofacScope;
		private readonly IDataBaseInfo dataBaseInfo;

		public byte ProductEdition { get; private set; }
		
		public ushort ClientId { get; private set; }
		
		public DateTime? ExpiryDate { get; private set; }

		private ushort employees_by_serial;
		public ushort Employees {
			get {
				if (employees_by_serial != 0)
					return employees_by_serial;
				if(ProductEdition == 1)
					return 50;
				if(ProductEdition == 2)
					return 500;
				return 0;
			}
		}

		public PaidFeatures PaidFeatures { get; private set; }

		public string CurrentEditionName => SupportEditions.First(x => x.Number == ProductEdition).Name;

		private bool failCloudConnection;
		private HashSet<string> availableCloudFeatures;
		

		private HashSet<string> AvailableCloudFeatures {
			get {
				if(availableCloudFeatures == null) {
					try {
						availableCloudFeatures = new HashSet<string>(
							cloudClientService.GetAvailableFeatures(dataBaseInfo.BaseGuid.Value.ToString())
								.Select(x => x.Name));
						failCloudConnection = false;
					}
					catch(Exception e) {
						if(failCloudConnection == false) {
							interactive.ShowMessage(ImportanceLevel.Error, "Облачный сервис не доступен. Функции связанные с облачным сервисом будут временно отключены, до возобновления связи.");
							failCloudConnection = true;
						}

						errorReporter.SendReport(e, ErrorType.Known);
						return new HashSet<string>();
					}
				}
				return availableCloudFeatures;
			}
		}

		public FeaturesService(ISerialNumberService serialNumberService,
			SerialNumberEncoder serialNumberEncoder,
			CloudClientService cloudClientService,
			IInteractiveMessage interactive,
			IErrorReporter errorReporter,
			ILifetimeScope autofacScope,
			IDataBaseInfo dataBaseInfo = null)
		{
			this.serialNumberEncoder = serialNumberEncoder ?? throw new ArgumentNullException(nameof(serialNumberEncoder));
			this.cloudClientService = cloudClientService ?? throw new ArgumentNullException(nameof(cloudClientService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
			this.autofacScope = autofacScope ?? throw new ArgumentNullException(nameof(autofacScope));
			this.dataBaseInfo = dataBaseInfo;
			if(dataBaseInfo?.IsDemo == true) {
				ProductEdition = 0;
				return;
			}

			ProductEdition = 1;
			 
			if(String.IsNullOrWhiteSpace(serialNumberService.SerialNumber))
				return;

			SetProperties(serialNumberService);
		}

		/// <summary>
		/// Используется только для тестов!!!
		/// </summary>
		public FeaturesService()
		{
		}

		private void SetProperties(ISerialNumberService serialNumberService)
		{
			serialNumberEncoder.Number = serialNumberService.SerialNumber;
			if(serialNumberEncoder.IsValid) {
				if(serialNumberEncoder.CodeVersion == 1)
					ProductEdition = 2; //Все купленные серийные номера версии 1 приравниваются к профессиональной редакции.
				else if((serialNumberEncoder.CodeVersion == 2 || serialNumberEncoder.CodeVersion == 3)
				        && serialNumberEncoder.EditionId >= 1
				        && serialNumberEncoder.EditionId <= 4) {
					ProductEdition = serialNumberEncoder.EditionId;
					ClientId = serialNumberEncoder.ClientId;
					ExpiryDate = serialNumberEncoder.ExpiryDate;
					employees_by_serial = serialNumberEncoder.Employees;
					PaidFeatures = (PaidFeatures)serialNumberEncoder.PaidFeaturesFags;
				}
			}
		}
		
		public void UpdateSerialNumber() 
		{
			ISerialNumberService serialNumberService = autofacScope.Resolve<ISerialNumberService>();
			SetProperties(serialNumberService);
		}
		
		public string GetEditionName(int editionId) 
		{
			return SupportEditions.FirstOrDefault(x => x.Number == editionId)?.Name;
		}
		
		public virtual bool Available(WorkwearFeature feature) 
		{
			if(feature.GetAttribute<IsCloudFeatureAttribute>() != null) {
				if(!cloudClientService.CanConnect)
					return false;
				if(dataBaseInfo.BaseGuid == null) {
					logger.Warn($"Облачная функциональность не доступна: dataBaseInfo.BaseGuid = null");
					return false;
				}

				switch(feature) {
					case WorkwearFeature.Communications:
					case WorkwearFeature.EmployeeLk:
						if(ProductEdition == 1)
							return false;
						return AvailableCloudFeatures.Contains("wear_lk");
					case WorkwearFeature.SpecCoinsLk:
						if(ProductEdition != 4) 
							return false;
						return AvailableCloudFeatures.Contains("speccoin_lk");
					case WorkwearFeature.Claims:
						if(ProductEdition != 0 && ProductEdition != 3 && ProductEdition != 4)
							return false;
						return AvailableCloudFeatures.Contains("claims_lk");
					case WorkwearFeature.Ratings:
						if(ProductEdition != 0 && ProductEdition != 3 && ProductEdition != 4)
							return false;
						return AvailableCloudFeatures.Contains("ratings");
					case WorkwearFeature.Postomats:
						if(ProductEdition != 0 && ProductEdition != 3 && ProductEdition != 4)
							return false;
						return AvailableCloudFeatures.Contains("postomats");
				}
			}

			switch(feature) {
				case WorkwearFeature.PrintPromo:
					return ProductEdition == 0 || ProductEdition == 1;
				//Только СпецАутсорсинг
				case WorkwearFeature.Selling:
				case WorkwearFeature.Dashboard:
				case WorkwearFeature.Shipment:
				case WorkwearFeature.Visits:
					return ProductEdition == 4;
				//Предприятие + СпецАутсорсинг
				case WorkwearFeature.BatchProcessing:
				case WorkwearFeature.CostCenter:
				case WorkwearFeature.EmployeeGroups:
				case WorkwearFeature.Exchange1C:
				case WorkwearFeature.ExportExcel:
				case WorkwearFeature.HistoryLog:
				case WorkwearFeature.Owners:
				case WorkwearFeature.ReportEmployeesReceived:
				case WorkwearFeature.ReportStockOperations:
				case WorkwearFeature.ReportSupply:
				case WorkwearFeature.StockForecasting:
				case WorkwearFeature.Warehouses:
					return ProductEdition == 0 || ProductEdition == 3 || ProductEdition == 4;
				// Платные функции предприятия
				case WorkwearFeature.IdentityCards:
					return ProductEdition == 0 || ProductEdition == 4 || (ProductEdition == 3 && PaidFeatures.HasFlag(PaidFeatures.IdentityCards));
				case WorkwearFeature.ClothingService:
					return ProductEdition == 0 || ProductEdition == 4 || (ProductEdition == 3 && PaidFeatures.HasFlag(PaidFeatures.ClothingService));
				case WorkwearFeature.Barcodes:
					return ProductEdition == 0 || ProductEdition == 4 || (ProductEdition == 3 && PaidFeatures.HasFlag(PaidFeatures.Barcodes));
				// Профессиональная + Предприятие + СпецАутсорсинг
				case WorkwearFeature.CollectiveExpense:
				case WorkwearFeature.Completion:
				case WorkwearFeature.ConditionNorm:
				case WorkwearFeature.CustomSizes:
				case WorkwearFeature.DutyNorms:
				case WorkwearFeature.EditLockDate:
				case WorkwearFeature.LoadExcel:
				case WorkwearFeature.ReportIssued:
				case WorkwearFeature.ReportOrder:
				case WorkwearFeature.ReportStock:
				case WorkwearFeature.ReportWearCard:
				case WorkwearFeature.ReportWrittenOff:
				case WorkwearFeature.StatementJournal:
				case WorkwearFeature.Vacation:
					return ProductEdition == 0 || ProductEdition == 2 || ProductEdition == 3 || ProductEdition == 4;
				// Профессиональная + Предприятие
				case WorkwearFeature.Inspection:
					return ProductEdition == 0 || ProductEdition == 2 || ProductEdition == 3;
				default:
					return false;
			}
		}
	}

	public enum WorkwearFeature
	{
		#region Однопользовательская
		[Display(Name="Промоданные")]
		PrintPromo,
		#endregion
		#region Профессиональная
		#region Документы
		[Display(Name = "Коллективная выдача")]
		CollectiveExpense,
		[Display(Name = "Комплектация")]
		Completion,
		[Display(Name = "Переоценки")]
		Inspection,
		[Display(Name = "Журнал ведомостей")]
		StatementJournal,
		#endregion
		[Display(Name = "Условия нормы")]
		ConditionNorm,
		[Display(Name = "Пользовательские размеры")]
		CustomSizes,
		[Display(Name = "Дежурные нормы")]
		DutyNorms,
		[Display(Name = "Дата запрета редактирования")]
		EditLockDate,
		[Display(Name = "Загрузка из Excel")]
		LoadExcel,
		[Display(Name = "Отпуска")]
		Vacation,
		#region Отчеты
		[Display(Name = "Отчет Складская ведомость")]
		ReportStock,
		[Display(Name = "Отчет Справка по выданному")]
		ReportIssued,
		[Display(Name = "Отчет Справка по списанному")]
		ReportWrittenOff,
		[Display(Name = "Отчет Заявка на спецодежду")]
		ReportOrder,
		[Display(Name="Отчет Список сотрудников")]
		ReportWearCard,
		#endregion
		#region С облаком
		[IsCloudFeature]
		[Display(Name = "Мобильный кабинет сотрудника")]
		EmployeeLk,
		[IsCloudFeature]
		[Display(Name = "Коммуникация с сотрудниками")]
		Communications,
		#endregion
		#endregion
		#region Предприятие
		[Display(Name = "Группы сотрудников")]
		EmployeeGroups,
		[Display(Name = "Выгрузка в Excel")]
		ExportExcel,
		[Display(Name = "Групповая обработка")]
		BatchProcessing,
		[Display(Name = "Работа с несколькими складами")]
		Warehouses,
		[Display(Name = "Собственники имущества")]
		Owners,
		[Display(Name = "Место возникновения затрат")]
		CostCenter,
		[Display(Name = "История изменений")]
		HistoryLog,
		[Display(Name = "Обмен с 1С")]
		Exchange1C,
		[Display(Name = "Прогнозирование запасов")]
		StockForecasting,
		#region Отчеты
		[Display(Name = "Отчет Справка по складским операциям")]
		ReportStockOperations,
		[Display(Name = "Отчет Количество получивших СИЗ")]
		ReportEmployeesReceived,
		[Display(Name = "Отчет по обеспеченности")]
		ReportSupply,
		#endregion
		#region Платные
		[Display(Name = "Маркировка(штрихкоды)")]
		Barcodes,
		[Display(Name = "Идентификация сотрудника по карте")]
		IdentityCards,
		[Display(Name = "Обслуживание спецодежды")]
		ClothingService,
		#endregion
		#region С облаком
		[IsCloudFeature]
		[Display(Name = "Обращения сотрудников")]
		Claims,
		[IsCloudFeature]
		[Display(Name = "Отзывы")]
		Ratings,
		#endregion
		#endregion
		#region Спецаутсорсинг
		[Display(Name = "Продажа")]
		Selling,
		[Display(Name = "Дашборды")]
		Dashboard,
		[Display(Name = "Поставки")]
		Shipment,
		[Display(Name = "Посещения склада")]
		Visits,
		#region С облаком
		[IsCloudFeature]
		[Display(Name = "Спецкойны")]
		SpecCoinsLk,
		[IsCloudFeature]
		[Display(Name = "Постаматы")]
		Postomats,
		#endregion
		#endregion
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class IsCloudFeatureAttribute : Attribute{ } 
}
