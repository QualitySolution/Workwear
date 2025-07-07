using System.ComponentModel.DataAnnotations;

namespace Workwear.Models.Analytics.WarehouseForecasting
{
	public enum ForecastingPriceType {
		[Display(Name = "Только количество")]
		None,
		// Пока не реализовано
		//[Display(Name = "Цена закупки")]
		//PurchasePrice,
		[Display(Name = "Оценочная стоимость")]
		AssessedCost,
		[Display(Name = "Цена продажи")]
		SalePrice
	}
}
