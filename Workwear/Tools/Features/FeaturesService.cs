using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Cloud.Client;
using QS.Project.Versioning;
using QS.Project.Versioning.Product;
using QS.Serial;
using QS.Serial.Encoding;

namespace workwear.Tools.Features
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
		private readonly IDataBaseInfo dataBaseInfo;

		public byte ProductEdition { get; }

		public string EditionName => SupportEditions.First(x => x.Number == ProductEdition).Name;

		public FeaturesService(ISerialNumberService serialNumberService, SerialNumberEncoder serialNumberEncoder, CloudClientService cloudClientService = null, IDataBaseInfo dataBaseInfo = null)
		{
			this.serialNumberEncoder = serialNumberEncoder ?? throw new ArgumentNullException(nameof(serialNumberEncoder));
			this.cloudClientService = cloudClientService ?? throw new ArgumentNullException(nameof(cloudClientService));
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
						&& serialNumberEncoder.EditionId <= 3) 
					ProductEdition = serialNumberEncoder.EditionId;
			}
		}

		/// <summary>
		/// Используется только для тестов!!!
		/// </summary>
		public FeaturesService()
		{
		}

		virtual public bool Available(WorkwearFeature feature)
		{
			if(ProductEdition == 0) //В демо редакции доступны все возможности кроме облачных
				return (feature != WorkwearFeature.Communications && feature != WorkwearFeature.EmployeeLk);

			switch(feature) {
				case WorkwearFeature.Warehouses:
				case WorkwearFeature.IdentityCards:
					return ProductEdition == 3;
				case WorkwearFeature.CollectiveExpense:
				case WorkwearFeature.LoadExcel:
				case WorkwearFeature.BatchProcessing:
					return ProductEdition == 2 || ProductEdition == 3;
				case WorkwearFeature.Communications:
				case WorkwearFeature.EmployeeLk:
					if(ProductEdition != 2 && ProductEdition != 3)
						return false;
					if(!QSSaaS.Session.IsSaasConnection)
						return false;
					if(dataBaseInfo.BaseGuid == null) {
						logger.Warn($"Функциональность мобильного кабинета не доступна: dataBaseInfo.BaseGuid = null");
						return false;
					}
					var functionLists = cloudClientService.GetAvailableFeatures(dataBaseInfo.BaseGuid.Value.ToString());
					return functionLists.Any(x => x.Name == "wear_lk");
				case WorkwearFeature.HistoryLog:
					return ProductEdition == 2 || ProductEdition == 3;
				case WorkwearFeature.Completion:
					return ProductEdition == 2 || ProductEdition == 3;
				default:
					return false;
			}
		}
	}

	public enum WorkwearFeature
	{
		[Display(Name = "Работа с несколькими складами")]
		Warehouses,
		[Display(Name = "Идентификация сотрудника по карте")]
		IdentityCards,
		[Display(Name = "Коллективная выдача")]
		CollectiveExpense,
		[Display(Name = "Загрузка из Excel")]
		LoadExcel,
		[Display(Name = "Групповая обработка")]
		BatchProcessing,
		[Display(Name = "Мобильный кабинет сотрудника")]
		EmployeeLk,
		[Display(Name = "Коммуникация с сотрудниками")]
		Communications,
		[Display(Name = "История изменений")]
		HistoryLog,
		[Display(Name = "Комплектация")]
		Completion
	}
}