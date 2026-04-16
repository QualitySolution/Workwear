using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Models.Operations;
using Workwear.Tools.Sizes;

namespace Workwear.Models.Analytics.WarehouseForecasting {
	public class NomenclatureForecastingModel : IForecastingModel {
		private readonly StockBalanceModel stockBalance;
		private readonly IForecastColumnsModel columnsModel;
		private readonly SizeService sizeService;
		public IEnumerable<Nomenclature> RestrictNomenclatures { get; set; }

		public NomenclatureForecastingModel(StockBalanceModel stockBalance, IForecastColumnsModel columnsModel, SizeService sizeService) {
			this.stockBalance = stockBalance ?? throw new ArgumentNullException(nameof(stockBalance));
			this.columnsModel = columnsModel ?? throw new ArgumentNullException(nameof(columnsModel));
			this.sizeService = sizeService ?? throw new ArgumentNullException(nameof(sizeService));
		}

		public List<WarehouseForecastingItem> MakeForecastingItems(IProgressBarDisplayable progress, List<FutureIssue> futureIssues) {
			progress.Start(4, text: "Группировка потребностей");
			var allNomenclatureSizes = sizeService.GetNomenclatureSizesCached().ToList();
			var groupsByProtectionTools = futureIssues.GroupBy(x => (x.ProtectionTools, x.Size, x.Height)).ToList();
			var result = new List<WarehouseForecastingItem>();
			progress.Add(text: "Группировка складских запасов");
			var groupsByNomenclature = stockBalance.Balances
				.Where(x => RestrictNomenclatures == null || RestrictNomenclatures.Contains(x.Position.Nomenclature))
				.GroupBy(x => (x.Position.Nomenclature, x.Position.WearSize, x.Position.Height)).ToList();
			foreach(var nomenclatureGroup in groupsByNomenclature) {
				result.Add(new WarehouseForecastingItem(columnsModel) {
					Nomenclature = nomenclatureGroup.Key.Nomenclature,
					Size = nomenclatureGroup.Key.WearSize,
					Height = nomenclatureGroup.Key.Height,
					StocksExact = nomenclatureGroup.ToArray(),
					InStock = nomenclatureGroup.Sum(x => x.Amount),
					Name = nomenclatureGroup.Key.Nomenclature.Name,
					Sex = nomenclatureGroup.Key.Nomenclature.Sex,
					SizeMatchStatus = SizeMatchStatus.Green
				});
			}
			
			progress.Add(text: "Суммирование расходов");
			foreach(var group in groupsByProtectionTools) {
				switch (group.Key.ProtectionTools.SupplyType)
				{
					case SupplyType.Unisex when group.Key.ProtectionTools.SupplyNomenclatureUnisex != null: {
						var nom = group.Key.ProtectionTools.SupplyNomenclatureUnisex;
						var (rs, rh, st) = WarehouseForecastingItem.ResolveSizeForNomenclature(nom, group.Key.Size, group.Key.Height, allNomenclatureSizes);
						AddForecastingItem(result, group.Key.ProtectionTools, nom, rs, rh, group, st);
						break;
					}
					case SupplyType.Unisex: {
						var (rs, rh, st) = WarehouseForecastingItem.ResolveSizeForNomenclature(null, group.Key.Size, group.Key.Height, allNomenclatureSizes);
						AddForecastingItem(result, group.Key.ProtectionTools, rs, rh, group, ClothesSex.Universal, st);
						break;
					}
					case SupplyType.TwoSex: {
						var mensIssues = group.OfType<FutureIssueEmployee>().Where(x => x.Employee.Sex == Sex.M).ToList<FutureIssue>();
						if(mensIssues.Any()) {
							if(group.Key.ProtectionTools.SupplyNomenclatureMale != null) {
								var nom = group.Key.ProtectionTools.SupplyNomenclatureMale;
								var (rs, rh, st) = WarehouseForecastingItem.ResolveSizeForNomenclature(nom, group.Key.Size, group.Key.Height, allNomenclatureSizes);
								AddForecastingItem(result, group.Key.ProtectionTools, nom, rs, rh, mensIssues, st);
							} else {
								var (rs, rh, st) = WarehouseForecastingItem.ResolveSizeForNomenclature(null, group.Key.Size, group.Key.Height, allNomenclatureSizes);
								AddForecastingItem(result, group.Key.ProtectionTools, rs, rh, mensIssues, ClothesSex.Men, st);
							}
						}
						var womenIssues = group.OfType<FutureIssueEmployee>().Where(x => x.Employee.Sex == Sex.F).ToList<FutureIssue>();
						if(womenIssues.Any()) {
							if(group.Key.ProtectionTools.SupplyNomenclatureFemale != null) {
								var nom = group.Key.ProtectionTools.SupplyNomenclatureFemale;
								var (rs, rh, st) = WarehouseForecastingItem.ResolveSizeForNomenclature(nom, group.Key.Size, group.Key.Height, allNomenclatureSizes);
								AddForecastingItem(result, group.Key.ProtectionTools, nom, rs, rh, womenIssues, st);
							} else {
								var (rs, rh, st) = WarehouseForecastingItem.ResolveSizeForNomenclature(null, group.Key.Size, group.Key.Height, allNomenclatureSizes);
								AddForecastingItem(result, group.Key.ProtectionTools, rs, rh, womenIssues, ClothesSex.Women, st);
							}
						}
						var universalIssues = group.Where(x => !(x is FutureIssueEmployee)).ToList();
						if(universalIssues.Any()) {
							if(group.Key.ProtectionTools.SupplyNomenclatureUnisex != null) {
								var nom = group.Key.ProtectionTools.SupplyNomenclatureUnisex;
								var (rs, rh, st) = WarehouseForecastingItem.ResolveSizeForNomenclature(nom, group.Key.Size, group.Key.Height, allNomenclatureSizes);
								AddForecastingItem(result, group.Key.ProtectionTools, nom, rs, rh, universalIssues, st);
							} else {
								var (rs, rh, st) = WarehouseForecastingItem.ResolveSizeForNomenclature(null, group.Key.Size, group.Key.Height, allNomenclatureSizes);
								AddForecastingItem(result, group.Key.ProtectionTools, rs, rh, universalIssues, ClothesSex.Universal, st);
							}
						}
						break;
					}
					default:
						throw new NotImplementedException();
				}
			}
			progress.Add(text: "Разбивка расходов по периодам");
			result.ForEach(x => x.FillForecast());
			progress.Close();
			return result;
		}

		private void AddForecastingItem(List<WarehouseForecastingItem> list, ProtectionTools protectionTools, Nomenclature nomenclature, Size size, Size height, IEnumerable<FutureIssue> futureIssues, SizeMatchStatus sizeStatus = SizeMatchStatus.Green) {
			if(RestrictNomenclatures != null && !RestrictNomenclatures.Contains(nomenclature))
				return;
			var item = list.FirstOrDefault(x => x.Nomenclature == nomenclature && x.Size == size && x.Height == height);
			if(item == null) {
				item = new WarehouseForecastingItem(columnsModel) {
					Nomenclature = nomenclature,
					Size = size,
					Height = height,
					StocksExact = Array.Empty<StockBalance>(),
					InStock = 0,
					Name = nomenclature.Name,
					Sex = nomenclature.Sex,
					SizeMatchStatus = sizeStatus
				};
				list.Add(item);
			} else {
				// Берём наихудший статус: Red > Orange > Green
				if(sizeStatus > item.SizeMatchStatus)
					item.SizeMatchStatus = sizeStatus;
			}
			item.AddFutureIssue(futureIssues);
			item.AddSuitableStock(stockBalance, protectionTools);
		}
		
		private void AddForecastingItem(List<WarehouseForecastingItem> list, ProtectionTools protectionTools, Size size, Size height,
			IEnumerable<FutureIssue> futureIssues, ClothesSex sex, SizeMatchStatus sizeStatus = SizeMatchStatus.Green) {
			if(RestrictNomenclatures != null)
				return;
			var item = new WarehouseForecastingItem(columnsModel) {
				ProtectionTool = protectionTools,
				Size = size,
				Height = height,
				StocksExact = Array.Empty<StockBalance>(),
				InStock = 0,
				Name = protectionTools.Name,
				Sex = sex,
				SupplyNomenclatureNotSet = true,
				SizeMatchStatus = sizeStatus
			};
			list.Add(item);
			item.AddFutureIssue(futureIssues);
			item.AddSuitableStock(stockBalance, protectionTools);
		}
	}
}
