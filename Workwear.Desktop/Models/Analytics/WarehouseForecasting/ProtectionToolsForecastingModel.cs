using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.Utilities.Text;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Models.Operations;
using Workwear.Tools.Sizes;

namespace Workwear.Models.Analytics.WarehouseForecasting {
	public class ProtectionToolsForecastingModel : IForecastingModel {
		private readonly StockBalanceModel stockBalance;
		private readonly IForecastColumnsModel columnsModel;
		private readonly SizeService sizeService;

		public ProtectionToolsForecastingModel(StockBalanceModel stockBalance, IForecastColumnsModel columnsModel, SizeService sizeService) {
			this.stockBalance = stockBalance ?? throw new ArgumentNullException(nameof(stockBalance));
			this.columnsModel = columnsModel ?? throw new ArgumentNullException(nameof(columnsModel));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
		}

		public List<WarehouseForecastingItem> MakeForecastingItems(IProgressBarDisplayable progress, List<FutureIssue> futureIssues) {
			var groups = futureIssues.GroupBy(x => (x.ProtectionTools, x.Size, x.Height)).ToList();
			var allNomenclatureSizes = sizeService.GetNomenclatureSizesCached().ToList();

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
				if (supplyType == SupplyType.Unisex) {
					var nomenclature = DetermineNomenclature(group.Key.ProtectionTools, ClothesSex.Universal, stocks);
					var (rs, rh, status) = WarehouseForecastingItem.ResolveSizeForNomenclature(nomenclature, group.Key.Size, group.Key.Height, allNomenclatureSizes);
					var resolvedKey = (group.Key.ProtectionTools, rs, rh);
					var item = new WarehouseForecastingItem(columnsModel, resolvedKey, group.ToList(), stocks, ClothesSex.Universal);
					item.SizeMatchStatus = status;
					result.Add(item);
				} else {
					var mensIssues = group.OfType<FutureIssueEmployee>().Where(x => x.Employee.Sex == Sex.M).ToList<FutureIssue>();
					if (mensIssues.Any()) {
						var nomenclature = DetermineNomenclature(group.Key.ProtectionTools, ClothesSex.Men, stocks);
						var (rs, rh, status) = WarehouseForecastingItem.ResolveSizeForNomenclature(nomenclature, group.Key.Size, group.Key.Height, allNomenclatureSizes);
						var resolvedKey = (group.Key.ProtectionTools, rs, rh);
						var item = new WarehouseForecastingItem(columnsModel, resolvedKey, mensIssues, stocks, ClothesSex.Men);
						item.SizeMatchStatus = status;
						result.Add(item);
					}
					var womenIssues = group.OfType<FutureIssueEmployee>().Where(x => x.Employee.Sex == Sex.F).ToList<FutureIssue>();
					if(womenIssues.Any()) {
						var nomenclature = DetermineNomenclature(group.Key.ProtectionTools, ClothesSex.Women, stocks);
						var (rs, rh, status) = WarehouseForecastingItem.ResolveSizeForNomenclature(nomenclature, group.Key.Size, group.Key.Height, allNomenclatureSizes);
						var resolvedKey = (group.Key.ProtectionTools, rs, rh);
						var item = new WarehouseForecastingItem(columnsModel, resolvedKey, womenIssues, stocks, ClothesSex.Women);
						item.SizeMatchStatus = status;
						result.Add(item);
					}
					var universalIssues = group.Where(x => !(x is FutureIssueEmployee)).ToList();
					if(universalIssues.Any()) {
						var nomenclature = DetermineNomenclature(group.Key.ProtectionTools, ClothesSex.Universal, stocks);
						var (rs, rh, status) = WarehouseForecastingItem.ResolveSizeForNomenclature(nomenclature, group.Key.Size, group.Key.Height, allNomenclatureSizes);
						var resolvedKey = (group.Key.ProtectionTools, rs, rh);
						var item = new WarehouseForecastingItem(columnsModel, resolvedKey, universalIssues, stocks, ClothesSex.Universal);
						item.SizeMatchStatus = status;
						result.Add(item);
					}
				}
			}
			progress.Close();
			return result;
		}

		/// <summary>Определяет номенклатуру для закупки для данного СИЗ и пола.</summary>
		private static Nomenclature DetermineNomenclature(ProtectionTools protectionTools, ClothesSex sex, StockBalance[] stocks) {
			if(sex == ClothesSex.Universal && protectionTools.SupplyNomenclatureUnisex != null)
				return protectionTools.SupplyNomenclatureUnisex;
			if(sex == ClothesSex.Men && protectionTools.SupplyNomenclatureMale != null)
				return protectionTools.SupplyNomenclatureMale;
			if(sex == ClothesSex.Women && protectionTools.SupplyNomenclatureFemale != null)
				return protectionTools.SupplyNomenclatureFemale;
			return stocks
				.Where(x => x.Position.Nomenclature.Sex == sex || x.Position.Nomenclature.Sex == ClothesSex.Universal)
				.OrderByDescending(x => x.Amount)
				.Select(x => x.Position.Nomenclature)
				.FirstOrDefault()
				?? protectionTools.Nomenclatures.FirstOrDefault();
		}
	}
}
