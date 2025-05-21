using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Models.Operations;
using Workwear.Tools.Sizes;

namespace Workwear.Models.Analytics.WarehouseForecasting {
	public class WarehouseForecastingItem : PropertyChangedBase {
		private readonly IForecastColumnsModel columnsModel;
		private List<FutureIssue> futureIssues;
		
		public WarehouseForecastingItem(
			IForecastColumnsModel model,
			(ProtectionTools ProtectionTools, Size Size, Size Height) key,
			List<FutureIssue> issues,
			StockBalance[] stocks,
			ClothesSex sex) {
			this.columnsModel = model ?? throw new ArgumentNullException(nameof(model));
			ProtectionTool = key.ProtectionTools;
			Size = key.Size;
			Height = key.Height;
			Sex = sex;
			Name = key.ProtectionTools.Name;
			futureIssues = issues;
			Stocks = stocks
				.Where(x => x.Position.Nomenclature.Sex == Sex || x.Position.Nomenclature.Sex == ClothesSex.Universal)
				.Where(x => SizeService.IsSuitable(Size, x.Position.WearSize))
				.Where(x => SizeService.IsSuitable(Height, x.Position.Height))
				.ToArray();
			InStock = Stocks.Sum(x => x.Amount);
			if(Sex == ClothesSex.Universal && ProtectionTool.SupplyNomenclatureUnisex != null)
				Nomenclature = ProtectionTool.SupplyNomenclatureUnisex;
			else if(Sex == ClothesSex.Men && ProtectionTool.SupplyNomenclatureMale != null)
				Nomenclature = ProtectionTool.SupplyNomenclatureMale;
			else if(Sex == ClothesSex.Women && ProtectionTool.SupplyNomenclatureFemale != null)
				Nomenclature = ProtectionTool.SupplyNomenclatureFemale;
			else
				Nomenclature = Stocks.OrderByDescending(x => x.Amount).Select(x => x.Position.Nomenclature).FirstOrDefault()
					?? ProtectionTool.Nomenclatures.FirstOrDefault();
			FillForecast();
		}

		public WarehouseForecastingItem(IForecastColumnsModel model) {
			this.columnsModel = model ?? throw new ArgumentNullException(nameof(model));
			futureIssues = new List<FutureIssue>();
		}
		
		public void AddFutureIssue(IEnumerable<FutureIssue> issues) {
			futureIssues.AddRange(issues);
		}
		
		#region Группировка
		public Nomenclature Nomenclature { get; set; }
		
		private ProtectionTools protectionTool;
		public ProtectionTools ProtectionTool {
			get => protectionTool;
			set => SetField(ref protectionTool, value);
		}
		
		private string name;
		public string Name {
			get => name;
			set => SetField(ref name, value);
		}
		
		private string[] suitableNomenclature = {};
		public string[] SuitableNomenclature {
			get => suitableNomenclature;
			set => SetField(ref suitableNomenclature, value);
		}

		private Size size;
		public Size Size {
			get => size;
			set => SetField(ref size, value);
		}

		private Size height;
		public Size Height {
			get => height;
			set => SetField(ref height, value);
		}
		
		public ClothesSex Sex { get; set; }
		#endregion

		public StockBalance[] Stocks { get; set; }
		
		private int inStock;
		public int InStock {
			get => inStock;
			set => SetField(ref inStock, value);
		}
		
		private int unissued;
		public int Unissued {
			get => unissued;
			set => SetField(ref unissued, value);
		}
		
		private int[] forecast;
		public int[] Forecast {
			get => forecast;
			set => SetField(ref forecast, value);
		}
		
		private int[] forecastBalance;
		public int[] ForecastBalance {
			get => forecastBalance;
			set => SetField(ref forecastBalance, value);
		}
		
		private string[] forecastColours;
		public string[] ForecastColours {
			get => forecastColours;
			set => SetField(ref forecastColours, value);
		}

		public bool SupplyNomenclatureNotSet;
		#region Рассчеты

		public void FillForecast() {
			Unissued = 0;
			Forecast = new int[columnsModel.ForecastColumns.Length];
			ForecastBalance = new int[columnsModel.ForecastColumns.Length];
			ForecastColours = new string[columnsModel.ForecastColumns.Length];
			foreach(var issue in futureIssues) {
				if(issue.DelayIssueDate < columnsModel.ForecastColumns[0].StartDate) {
					Unissued += issue.Amount;
					continue;
				}

				for(int i = 0; i < columnsModel.ForecastColumns.Length; i++) {
					var issueDate = issue.DelayIssueDate ?? issue.OperationDate;
					if(issueDate >= columnsModel.ForecastColumns[i].StartDate && issueDate <= columnsModel.ForecastColumns[i].EndDate) {
						Forecast[i] += issue.Amount;
						break;
					}
				}
			}
			//Раскраска
			for(int i = 0; i < columnsModel.ForecastColumns.Length; i++) {
				var onStock = InStock - Forecast.Take(i + 1).Sum();
				ForecastBalance[i] = onStock;
				if(onStock < 0) {
					ForecastColours[i] = "red";
				} else if(onStock - Unissued < 0) {
					ForecastColours[i] = "orange";
				} else {
					ForecastColours[i] = "green";
				}
			}
		}

		#endregion

		#region Расчетные для отображения
		public decimal GetPrice(ForecastingPriceType priceType) {
			switch(priceType) {
				case ForecastingPriceType.None:
					return 0;
				case ForecastingPriceType.SalePrice:
					return Nomenclature?.SaleCost ?? 0;
				case ForecastingPriceType.AssessedCost:
					return ProtectionTool?.AssessedCost ?? 0;
				default:
					throw new NotImplementedException();
			}
		}
		public string NameColor => Nomenclature == null ? "blue" : "black";
		public string SizeText => SizeService.SizeTitle(size, height);
		public int ClosingBalance => InStock - Unissued - Forecast.Sum();
		public string NomenclaturesText {
			get {
				string text = "";
				if(SupplyNomenclatureNotSet)
					text += "Номенклатура для закупки не установлена! В прогнозе используется номенклатура нормы.\n";
				if (SuitableNomenclature.Any())
					text += "Подходящие номенклатуры:" + String.Concat(SuitableNomenclature.Select(x => $"\n* {x}"));
				return String.IsNullOrEmpty(text) ? null : text.Trim();
			}
		}

		public string StockText => Stocks.Any() ? "В наличии:" + String.Concat(Stocks.Select(x => $"\n{x.Position.Nomenclature.GetAmountAndUnitsText(x.Amount)} — {x.Position.Title}")) : null;
		#endregion
	}
}
