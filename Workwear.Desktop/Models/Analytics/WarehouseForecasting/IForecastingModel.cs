using System.Collections.Generic;
using QS.Dialog;

namespace Workwear.Models.Analytics.WarehouseForecasting {
	public interface IForecastingModel {
		List<WarehouseForecastingItem> MakeForecastingItems(IProgressBarDisplayable progress, List<FutureIssue> futureIssues);
	}
}
