using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using Workwear.Domain.Regulations;
using Workwear.Domain.Sizes;
using Workwear.Models.Analytics;
using Workwear.Models.Operations;
using Workwear.Tools.Sizes;

namespace Workwear.ViewModels.Analytics {
	public class WarehouseForecastingItem : PropertyChangedBase {
		private readonly WarehouseForecastingViewModel model;
		private List<FutureIssue> futureIssues;
		
		public WarehouseForecastingItem(
			WarehouseForecastingViewModel model,
			(ProtectionTools ProtectionTools, Size Size, Size Height) key,
			List<FutureIssue> issues,
			StockBalance[] stocks,
			ClothesSex sex) {
			this.model = model ?? throw new ArgumentNullException(nameof(model));
			ProtectionTool = key.ProtectionTools;
			Size = key.Size;
			Height = key.Height;
			Sex = sex;
			futureIssues = issues;
			InStock = stocks
				.Where(x => x.Position.Nomenclature.Sex == Sex)
				.Where(x => x.Position.WearSize == Size && x.Position.Height == Height)
				.Sum(x => x.Amount);

			FillForecast();			
		}
		
		#region Группировка
		private ProtectionTools protectionTool;
		public ProtectionTools ProtectionTool {
			get => protectionTool;
			set => SetField(ref protectionTool, value);
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
		
		private string[] forecastColours;
		public string[] ForecastColours {
			get => forecastColours;
			set => SetField(ref forecastColours, value);
		}

		#region Рассчеты

		public void FillForecast() {
			Unissued = 0;
			Forecast = new int[model.ForecastColumns.Length];
			ForecastColours = new string[model.ForecastColumns.Length];
			foreach(var issue in futureIssues) {
				if(issue.DelayIssueDate < model.ForecastColumns[0].StartDate) {
					Unissued += issue.Amount;
					continue;
				}

				for(int i = 0; i < model.ForecastColumns.Length; i++) {
					var issueDate = issue.DelayIssueDate ?? issue.OperationDate;
					if(issueDate >= model.ForecastColumns[i].StartDate && issueDate <= model.ForecastColumns[i].EndDate) {
						Forecast[i] += issue.Amount;
						break;
					}
				}
			}
			//Раскраска
			for(int i = 0; i < model.ForecastColumns.Length; i++) {
				var onStock = InStock - Forecast.Take(i + 1).Sum();
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

		public string SizeText => SizeService.SizeTitle(size, height);

		#endregion
	}
}
