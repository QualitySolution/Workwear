using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.Utilities.Text;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Models.Operations;

namespace Workwear.Models.Analytics.WarehouseForecasting {
	public class ProtectionToolsForecastingModel : IForecastingModel {
		private readonly StockBalanceModel stockBalance;
		private readonly IForecastColumnsModel columnsModel;

		public ProtectionToolsForecastingModel(StockBalanceModel stockBalance, IForecastColumnsModel columnsModel) {
			this.stockBalance = stockBalance ?? throw new ArgumentNullException(nameof(stockBalance));
			this.columnsModel = columnsModel ?? throw new ArgumentNullException(nameof(columnsModel));
		}

		public List<WarehouseForecastingItem> MakeForecastingItems(IProgressBarDisplayable progress, List<FutureIssue> futureIssues) {
			var groups = futureIssues.GroupBy(x => (x.ProtectionTools, x.Size, x.Height)).ToList();
			
			progress.Start(groups.Count() + 1, text: "Суммирование");
			var result = new List<WarehouseForecastingItem>();
			foreach(var group in groups) {
				progress.Add(text: group.Key.ProtectionTools.Name.EllipsizeMiddle(100));
				var stocks = stockBalance.ForNomenclature(group.Key.ProtectionTools.Nomenclatures.ToArray()).ToArray();
				SupplyType supplyType; 
				if(group.Key.ProtectionTools.SupplyType == SupplyType.Unisex && group.Key.ProtectionTools.SupplyNomenclatureUnisex != null)
					supplyType = SupplyType.Unisex;
				else if(group.Key.ProtectionTools.SupplyType == SupplyType.TwoSex && (group.Key.ProtectionTools.SupplyNomenclatureMale != null || group.Key.ProtectionTools.SupplyNomenclatureFemale != null))
					supplyType = SupplyType.TwoSex;
				else
					supplyType = (stocks.OrderByDescending(x => x.Amount).FirstOrDefault()?.Position.Nomenclature.Sex ?? ClothesSex.Universal) == ClothesSex.Universal ? SupplyType.Unisex : SupplyType.TwoSex;
				if (supplyType == SupplyType.Unisex)
					result.Add(new WarehouseForecastingItem(columnsModel, group.Key, group.ToList(), stocks, ClothesSex.Universal));
				else {
					var mensIssues = group.Where(x => x.Employee.Sex == Sex.M).ToList();
					if (mensIssues.Any())
						result.Add(new WarehouseForecastingItem(columnsModel, group.Key, mensIssues, stocks, ClothesSex.Men));
					var womenIssues = group.Where(x => x.Employee.Sex == Sex.F).ToList();
					if(womenIssues.Any())
						result.Add(new WarehouseForecastingItem(columnsModel, group.Key, womenIssues, stocks, ClothesSex.Women));
				}
			}

			return result;
		}
	}
}
