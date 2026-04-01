using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Supply;
using Workwear.Models.Operations;
using Workwear.Tools.Sizes;

namespace Workwear.Models.Analytics.WarehouseForecasting {
	/// <summary>
	/// Статус подбора размера номенклатуры по размеру сотрудника.
	/// </summary>
	public enum SizeMatchStatus {
		/// <summary>Размер найден в <see cref="Nomenclature.NomenclatureSizes"/> конкретной номенклатуры или среди общих размеров номенклатур.</summary>
		Green = 0,
		/// <summary>Найден общий размер номенклатуры, но комбинация не добавлена в конкретную номенклатуру.</summary>
		Orange = 1,
		/// <summary>Подходящий размер номенклатуры не найден — оставлен размер сотрудника.</summary>
		Red = 2
	}

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
			StocksExact = stocks
				.Where(x => x.Position.Nomenclature.Sex == Sex || x.Position.Nomenclature.Sex == ClothesSex.Universal)
				.Where(x => SizeService.IsSuitable(Size, x.Position.WearSize))
				.Where(x => SizeService.IsSuitable(Height, x.Position.Height))
				.ToArray();
			InStock = StocksExact.Sum(x => x.Amount);
			if(Sex == ClothesSex.Universal && ProtectionTool.SupplyNomenclatureUnisex != null)
				Nomenclature = ProtectionTool.SupplyNomenclatureUnisex;
			else if(Sex == ClothesSex.Men && ProtectionTool.SupplyNomenclatureMale != null)
				Nomenclature = ProtectionTool.SupplyNomenclatureMale;
			else if(Sex == ClothesSex.Women && ProtectionTool.SupplyNomenclatureFemale != null)
				Nomenclature = ProtectionTool.SupplyNomenclatureFemale;
			else
				Nomenclature = StocksExact.OrderByDescending(x => x.Amount).Select(x => x.Position.Nomenclature).FirstOrDefault()
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

		private SizeMatchStatus sizeMatchStatus = SizeMatchStatus.Green;
		/// <summary>Статус подбора размера номенклатуры по размеру сотрудника.</summary>
		public SizeMatchStatus SizeMatchStatus {
			get => sizeMatchStatus;
			set => SetField(ref sizeMatchStatus, value);
		}
		/// <summary>Цвет для отображения размера в зависимости от статуса подбора.</summary>
		public string SizeStatusColor =>
			SizeMatchStatus == SizeMatchStatus.Red ? "red" :
			SizeMatchStatus == SizeMatchStatus.Orange ? "orange" :
			"darkgreen";
		#endregion

		public StockBalance[] StocksExact { get; set; }
		public HashSet<StockBalance> StocksSuitable { get; set; } = new HashSet<StockBalance>();
		
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

		public List<FutureIssue> UnissuedIssues = new List<FutureIssue>();
		
		private readonly List<ShipmentItem> shipmentItems = new List<ShipmentItem>();
		public List<ShipmentItem> ShipmentItems => shipmentItems;
		
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
		
		private List<FutureIssue>[] forecastIssues;
		public List<FutureIssue>[] ForecastIssues {
			get => forecastIssues;
			set => SetField(ref forecastIssues, value);
		}

		public bool SupplyNomenclatureNotSet;

		#region Добавление данных
		public void AddSuitableStock(StockBalanceModel stockBalance, ProtectionTools protectionToolToAdd) {
			var nomenclatures = protectionToolToAdd.Nomenclatures
				.Where(n => n.Sex == Sex || n.Sex == ClothesSex.Universal)
				.ToArray();
			var stock = stockBalance.ForNomenclature(nomenclatures)
				.Where(x => SizeService.IsSuitable(Size, x.Position.WearSize))
				.Where(x => SizeService.IsSuitable(Height, x.Position.Height));
			StocksSuitable.UnionWith(stock);
		}
		#endregion
		
		#region Разрешение размера
		/// <summary>
		/// Разрешает размер сотрудника в размер номенклатуры с учётом <see cref="Nomenclature.NomenclatureSizes"/>.
		/// <para>
		/// Если <see cref="Nomenclature.NomenclatureSizes"/> не пуст — ищет совпадение только среди зарегистрированных
		/// размеров: точное или через <see cref="Size.SuitableSizes"/>. Статус <see cref="SizeMatchStatus.Green"/>.
		/// Если совпадение не найдено — статус <see cref="SizeMatchStatus.Red"/>.
		/// </para>
		/// <para>
		/// Если <see cref="Nomenclature.NomenclatureSizes"/> пуст — ищет подходящий размер среди всех
		/// <paramref name="allNomenclatureSizes"/> (ShowInNomenclature=true). Статус <see cref="SizeMatchStatus.Green"/>.
		/// Если найден размер из ShowInNomenclature, но NomenclatureSizes была не пуста — статус <see cref="SizeMatchStatus.Orange"/>.
		/// Если ничего не найдено — оставляется размер сотрудника, статус <see cref="SizeMatchStatus.Red"/>.
		/// </para>
		/// Важно: если есть рост, совпадение ищется по паре размер+рост вместе.
		/// </summary>
		public static (Size size, Size height, SizeMatchStatus status) ResolveSizeForNomenclature(
			Nomenclature nomenclature, Size employeeSize, Size employeeHeight,
			IEnumerable<Size> allNomenclatureSizes) {

			var nomenclatureSizesList = nomenclature?.NomenclatureSizes;
			bool hasNomenclatureSizes = nomenclatureSizesList?.Any() == true;

			if(hasNomenclatureSizes) {
				// Ищем совпадение пары (размер + рост) среди зарегистрированных вариантов номенклатуры
				var match = nomenclatureSizesList.FirstOrDefault(ns =>
					SizeService.IsSuitable(employeeSize, ns.Size) &&
					SizeService.IsSuitable(employeeHeight, ns.Height));
				if(match != null)
					return (match.Size, match.Height, SizeMatchStatus.Green);
			}

			// Ищем через ShowInNomenclature размеры
			if(allNomenclatureSizes != null) {
				var nomSizesList = allNomenclatureSizes as IList<Size> ?? allNomenclatureSizes.ToList();
				if(nomSizesList.Any()) {
					// Для размера: фильтруем по типу размера сотрудника
					Size resolvedSize = employeeSize == null ? null
						: nomSizesList
							.Where(s => s.SizeType.Id == employeeSize.SizeType.Id)
							.FirstOrDefault(s => SizeService.IsSuitable(employeeSize, s));

					// Для роста: фильтруем по типу размера сотрудника
					Size resolvedHeight = employeeHeight == null ? null
						: nomSizesList
							.Where(s => s.SizeType.Id == employeeHeight.SizeType.Id)
							.FirstOrDefault(h => SizeService.IsSuitable(employeeHeight, h));

					bool sizeOk = employeeSize == null || resolvedSize != null;
					bool heightOk = employeeHeight == null || resolvedHeight != null;

					if(sizeOk && heightOk) {
						// Если NomenclatureSizes была не пуста — мы нашли размер не в ней → Orange
						var status = hasNomenclatureSizes ? SizeMatchStatus.Orange : SizeMatchStatus.Green;
						return (resolvedSize, resolvedHeight, status);
					}
				}
			}

			// Ничего не нашли — оставляем размер сотрудника
			return (employeeSize, employeeHeight, SizeMatchStatus.Red);
		}
		#endregion

		#region Рассчеты
		public void FillForecast() {
			Unissued = 0;
			UnissuedIssues.Clear();
			Forecast = new int[columnsModel.ForecastColumns.Length];
			ForecastBalance = new int[columnsModel.ForecastColumns.Length];
			ForecastColours = new string[columnsModel.ForecastColumns.Length];
			ForecastIssues = new List<FutureIssue>[columnsModel.ForecastColumns.Length];
			for(int i = 0; i < columnsModel.ForecastColumns.Length; i++) {
				ForecastIssues[i] = new List<FutureIssue>();
			}
			foreach(var issue in futureIssues) {
				if(issue.DelayIssueDate < columnsModel.ForecastColumns[0].StartDate) {
					UnissuedIssues.Add(issue);
					Unissued += issue.Amount;
					continue;
				}

				for(int i = 0; i < columnsModel.ForecastColumns.Length; i++) {
					var issueDate = issue.DelayIssueDate ?? issue.OperationDate;
					if(issueDate >= columnsModel.ForecastColumns[i].StartDate && issueDate <= columnsModel.ForecastColumns[i].EndDate) {
						ForecastIssues[i].Add(issue);
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
		public int InStockSuitable => StocksSuitable.Sum(x => x.Amount);
		public int TotalOrdered => ShipmentItems.Sum(x => x.OrderedNotReceived);
		public int WithDebt => InStock + TotalOrdered - Unissued - Forecast.Sum();
		public int WithoutDebt => InStock + TotalOrdered - Forecast.Sum();
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

		public string StockText => StocksExact.Any() ? "В наличии:" + String.Concat(StocksExact.Select(x => $"\n{x.Position.Nomenclature.GetAmountAndUnitsText(x.Amount)} — {x.Position.Title}")) : null;
		public string StocksSuitableText => StocksSuitable.Any() ? "В наличии:" + String.Concat(StocksSuitable.Select(x => $"\n{x.Position.Nomenclature.GetAmountAndUnitsText(x.Amount)} — {x.Position.Title}")) : null;
		#endregion
	}
}
