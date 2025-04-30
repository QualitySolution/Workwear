using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Models.Operations;

namespace Workwear.Models.Analytics.WarehouseForecasting {
	public class NomenclatureForecastingModel : IForecastingModel {
		private readonly StockBalanceModel stockBalance;
		private readonly IForecastColumnsModel columnsModel;
		public IEnumerable<Nomenclature> RestrictNomenclatures { get; set; }

		public NomenclatureForecastingModel(StockBalanceModel stockBalance, IForecastColumnsModel columnsModel) {
			this.stockBalance = stockBalance ?? throw new ArgumentNullException(nameof(stockBalance));
			this.columnsModel = columnsModel ?? throw new ArgumentNullException(nameof(columnsModel));
		}

		public List<WarehouseForecastingItem> MakeForecastingItems(IProgressBarDisplayable progress, List<FutureIssue> futureIssues) {
			progress.Start(10, text: "Группировка потребностей");
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
					Stocks = nomenclatureGroup.ToArray(),
					InStock = nomenclatureGroup.Sum(x => x.Amount),
					Name = nomenclatureGroup.Key.Nomenclature.Name,
					Sex = nomenclatureGroup.Key.Nomenclature.Sex
				});
			}
			
			progress.Add(text: "Суммирование расходов");
			foreach(var group in groupsByProtectionTools) {
				switch (group.Key.ProtectionTools.SupplyType)
				{
					case SupplyType.Unisex when group.Key.ProtectionTools.SupplyNomenclatureUnisex != null:
						AddForecastingItem(result, group.Key.ProtectionTools.SupplyNomenclatureUnisex, group.Key.Size, group.Key.Height, group);
						break;
					case SupplyType.Unisex:
						AddForecastingItem(result, group.Key.ProtectionTools, group.Key.Size, group.Key.Height, group, ClothesSex.Universal);
						break;
					case SupplyType.TwoSex:
						var mensIssues = group.Where(x => x.Employee.Sex == Sex.M).ToList();
						if(mensIssues.Any()) {
							if(group.Key.ProtectionTools.SupplyNomenclatureMale != null)
								AddForecastingItem(result, group.Key.ProtectionTools.SupplyNomenclatureMale, group.Key.Size, group.Key.Height, mensIssues);
							else 
								AddForecastingItem(result, group.Key.ProtectionTools, group.Key.Size, group.Key.Height, mensIssues, ClothesSex.Men);
						}
						var womenIssues = group.Where(x => x.Employee.Sex == Sex.F).ToList();
						if(womenIssues.Any()) {
							if(group.Key.ProtectionTools.SupplyNomenclatureFemale != null)
								AddForecastingItem(result, group.Key.ProtectionTools.SupplyNomenclatureFemale, group.Key.Size, group.Key.Height, womenIssues);
							else 
								AddForecastingItem(result, group.Key.ProtectionTools, group.Key.Size, group.Key.Height, womenIssues, ClothesSex.Women);
						}
						break;
					default:
						throw new NotImplementedException();
				}
			}
			progress.Add(text: "Разбивка расходов по периодам");
			result.ForEach(x => x.FillForecast());

			return result;
		}

		private void AddForecastingItem(List<WarehouseForecastingItem> list, Nomenclature nomenclature, Size size, Size height, IEnumerable<FutureIssue> futureIssues) {
			if(RestrictNomenclatures != null && !RestrictNomenclatures.Contains(nomenclature))
				return;
			var item = list.FirstOrDefault(x => x.Nomenclature == nomenclature && x.Size == size && x.Height == height);
			if(item == null) {
				item = new WarehouseForecastingItem(columnsModel) {
					Nomenclature = nomenclature,
					Size = size,
					Height = height,
					Stocks = Array.Empty<StockBalance>(),
					InStock = 0,
					Name = nomenclature.Name,
					Sex = nomenclature.Sex
				};
				list.Add(item);
			}
			item.AddFutureIssue(futureIssues);
		}
		
		private void AddForecastingItem(List<WarehouseForecastingItem> list, ProtectionTools protectionTools, Size size, Size height,
			IEnumerable<FutureIssue> futureIssues, ClothesSex sex) {
			if(RestrictNomenclatures != null)
				return;
			var item = new WarehouseForecastingItem(columnsModel) {
				ProtectionTool = protectionTools,
				Size = size,
				Height = height,
				Stocks = Array.Empty<StockBalance>(),
				InStock = 0,
				Name = protectionTools.Name,
				Sex = sex,
				SupplyNomenclatureNotSet = true
			};
			list.Add(item);
			item.AddFutureIssue(futureIssues);
		}
	}
}
