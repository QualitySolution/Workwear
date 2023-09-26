﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
			new ProductEdition(3, "Предприятие")
		};
		private readonly SerialNumberEncoder serialNumberEncoder;
		private readonly CloudClientService cloudClientService;
		private readonly IInteractiveMessage interactive;
		private readonly IErrorReporter errorReporter;
		private readonly IDataBaseInfo dataBaseInfo;

		public byte ProductEdition { get; }
		public ushort ClientId { get; }

		public string EditionName => SupportEditions.First(x => x.Number == ProductEdition).Name;


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
			IDataBaseInfo dataBaseInfo = null)
		{
			this.serialNumberEncoder = serialNumberEncoder ?? throw new ArgumentNullException(nameof(serialNumberEncoder));
			this.cloudClientService = cloudClientService ?? throw new ArgumentNullException(nameof(cloudClientService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.errorReporter = errorReporter ?? throw new ArgumentNullException(nameof(errorReporter));
			this.dataBaseInfo = dataBaseInfo;
			if(dataBaseInfo?.IsDemo == true) {
				ProductEdition = 0;
				return;
			}

			ProductEdition = 1;
			 
			if(String.IsNullOrWhiteSpace(serialNumberService.SerialNumber))
				return;

			serialNumberEncoder.Number = serialNumberService.SerialNumber;
			if(serialNumberEncoder.IsValid) {
				if(serialNumberEncoder.CodeVersion == 1)
					ProductEdition = 2; //Все купленные серийные номера версии 1 приравниваются к профессиональной редакции.
				else if(serialNumberEncoder.CodeVersion == 2
				        && serialNumberEncoder.EditionId >= 1
				        && serialNumberEncoder.EditionId <= 3) {
					ProductEdition = serialNumberEncoder.EditionId;
					ClientId = serialNumberEncoder.ClientId;
				}
			}
		}

		/// <summary>
		/// Используется только для тестов!!!
		/// </summary>
		public FeaturesService()
		{
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
						if(ProductEdition != 0 && ProductEdition != 2 && ProductEdition != 3)
							return false;
						
						return AvailableCloudFeatures.Contains("wear_lk");
					case WorkwearFeature.Claims:
						if(ProductEdition != 0 && ProductEdition != 3)
							return false;
						return AvailableCloudFeatures.Contains("claims_lk");
					case WorkwearFeature.Ratings:
						if(ProductEdition != 0 && ProductEdition != 3)
							return false;
						return AvailableCloudFeatures.Contains("ratings");
					case WorkwearFeature.Postomats:
						if(ProductEdition != 0 && ProductEdition != 3)
							return false;
						return AvailableCloudFeatures.Contains("postomats");
				}
			}

			switch(feature) {
				#if	DEBUG //Пока доступно только в редакции спецпошива
				case WorkwearFeature.Selling:
				#endif
				case WorkwearFeature.Barcodes:
				case WorkwearFeature.Warehouses:
				case WorkwearFeature.IdentityCards:
				case WorkwearFeature.Owners:
				case WorkwearFeature.CostCenter:
				case WorkwearFeature.Exchange1C:
					return ProductEdition == 0 || ProductEdition == 3;
				case WorkwearFeature.CollectiveExpense:
				case WorkwearFeature.Completion:
				case WorkwearFeature.Inspection:
				case WorkwearFeature.LoadExcel:
				case WorkwearFeature.BatchProcessing:
				case WorkwearFeature.HistoryLog:
				case WorkwearFeature.ConditionNorm:
				case WorkwearFeature.CustomSizes:
					return ProductEdition == 0 || ProductEdition == 2 || ProductEdition == 3;
				default:
					return false;
			}
		}
	}

	public enum WorkwearFeature
	{
		#region Профессиональная
		[Display(Name = "Коллективная выдача")]
		CollectiveExpense,
		[Display(Name = "Комплектация")]
		Completion,
		[Display(Name = "Переоценки")]
		Inspection,
		[Display(Name = "Загрузка из Excel")]
		LoadExcel,
		[Display(Name = "Групповая обработка")]
		BatchProcessing,
		[Display(Name = "История изменений")]
		HistoryLog,
		[Display(Name = "Условия нормы")]
		ConditionNorm,
		[Display(Name = "Пользовательские размеры")]
		CustomSizes,
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
		[Display(Name = "Работа с несколькими складами")]
		Warehouses,
		[Display(Name = "Идентификация сотрудника по карте")]
		IdentityCards,
		[Display(Name = "Собственники имущества")]
		Owners,
		[Display(Name = "Место возникновения затрат")]
		CostCenter,
		[Display(Name = "Штрихкоды")]
		Barcodes,
		[Display(Name = "Обмен с 1С")]
		Exchange1C,
		#region С облаком
		[IsCloudFeature]
		[Display(Name = "Обращения сотрудников")]
		Claims,
		[IsCloudFeature]
		[Display(Name = "Отзывы")]
		Ratings,
		[IsCloudFeature]
		[Display(Name = "Постоматы")]
		Postomats,
		#endregion
		#endregion
		#region Спецредакции
		[Display(Name = "Продажа")]
		Selling
		#endregion
	}
	
	[AttributeUsage(AttributeTargets.Field)]
	public class IsCloudFeatureAttribute : Attribute{ } 
}
