using System;

namespace Workwear.Models.Analytics.WarehouseForecasting {
	public interface IForecastColumnsModel {
		ForecastColumn[] ForecastColumns { get; }
	}
	
	public class ForecastColumn {
		public string Title { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}
