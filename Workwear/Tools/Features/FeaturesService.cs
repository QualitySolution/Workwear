using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Project.Versioning;
using QS.Project.Versioning.Product;
using QS.Serial;
using QS.Serial.Encoding;

namespace workwear.Tools.Features
{
	public class FeaturesService : IProductService
	{
		public static ProductEdition[] SupportEditions = new[] {
			new ProductEdition(0, "Демонстрационная"),
			new ProductEdition(1, "Однопользовательская"),
			new ProductEdition(2, "Профессиональная"),
			new ProductEdition(3, "Предприятие")
		};
		private readonly SerialNumberEncoder serialNumberEncoder;
		private readonly IDataBaseInfo dataBaseInfo;

		public byte ProductEdition { get; }

		public string EditionName => SupportEditions.First(x => x.Number == ProductEdition).Name;

		public FeaturesService(ISerialNumberService serialNumberService, SerialNumberEncoder serialNumberEncoder, IDataBaseInfo dataBaseInfo = null)
		{
			this.serialNumberEncoder = serialNumberEncoder ?? throw new ArgumentNullException(nameof(serialNumberEncoder));
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
					ProductEdition = 2; //Все купленные серийные номера версии 1 приравниваются к професиональной редации.
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
			if(ProductEdition == 0)
				return true; //В демо редакции доступны все возможности.

			switch(feature) {
				case WorkwearFeature.Warehouses:
					return ProductEdition == 3;
				case WorkwearFeature.IdentityCards:
					return ProductEdition == 3;
				case WorkwearFeature.MassExpense:
					return ProductEdition == 2 || ProductEdition == 3;
				case WorkwearFeature.LoadExcel:
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
		[Display(Name = "Выдача списком")]
		MassExpense,
		[Display(Name = "Загрузка из Excel")]
		LoadExcel
	}
}
