﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using workwear.ViewModels.Import;

namespace workwear.Models.Import.Issuance
{
	public class ImportModelWorkwearItems : ImportModelBase<DataTypeWorkwearItems, SheetRowWorkwearItems>, IImportModel
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly DataParserWorkwearItems dataParser;
		private readonly SettingsWorkwearItemsViewModel settingsWorkwearItemsViewModel;

		public ImportModelWorkwearItems(DataParserWorkwearItems dataParser, SettingsWorkwearItemsViewModel settingsWorkwearItemsViewModel) : base(dataParser, typeof(CountersWorkwearItems), settingsWorkwearItemsViewModel)
		{
			this.dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
			this.settingsWorkwearItemsViewModel = settingsWorkwearItemsViewModel ?? throw new ArgumentNullException(nameof(settingsWorkwearItemsViewModel));
		}

		#region Параметры
		public string ImportName => "Загрузка выдачи";

		public string DataColumnsRecommendations => "Установите номер строки с заголовком данных, таким образом чтобы название колонок было корректно. Если в таблице заголовки отсутствуют укажите 0.\nДалее для каждой значимой колонки проставьте тип данных которые находится в таблице.\nОбязательными данными являются Табельный номер, Номенклатура нормы и выдачи, Дата и количество выдачи";

		#endregion
		protected override DataTypeWorkwearItems[] RequiredDataTypes => new []{DataTypeWorkwearItems.ProtectionTools, DataTypeWorkwearItems.Nomenclature, DataTypeWorkwearItems.Count, DataTypeWorkwearItems.IssueDate};

		protected override bool HasRequiredDataTypes(IEnumerable<DataTypeWorkwearItems> dataTypes) {
			return (dataTypes.Contains(DataTypeWorkwearItems.PersonnelNumber) ||dataTypes.Contains(DataTypeWorkwearItems.Fio)) 
			       && base.HasRequiredDataTypes(dataTypes);
		}

		public bool CanSave { get; private set; }

		public List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			var countColumn = ImportedDataTypes.First(x => DataTypeWorkwearItems.Count.Equals(x.DataType.Data));
			var grouped = UsedRows.Where(x => x.Operation != null)
				.GroupBy(x => x.Employee);
			logger.Debug($"В обработке {grouped.Count()} сотрудников.");
			progress.Start(maxValue: grouped.Count(), text: "Подготовка");
			foreach(var employeeGroup in grouped) {
				progress.Add(text: $"Подготовка {employeeGroup.Key.ShortName}");
				var rowByItem = employeeGroup.GroupBy(x => x.WorkwearItem);
				foreach(var itemGroup in rowByItem) {
					var last = itemGroup.OrderByDescending(x => x.Date).First();
					if(itemGroup.Key.LastIssue == null || itemGroup.Key.LastIssue < last.Date) {
						itemGroup.Key.LastIssue = last.Date;
						itemGroup.Key.Amount = last.CellIntValue(countColumn).Value;
						itemGroup.Key.NextIssue = itemGroup.Key.ActiveNormItem.CalculateExpireDate(last.Date.Value, itemGroup.Key.Amount);
						dataParser.ChangedEmployees.Add(employeeGroup.Key);
					}
				}
			}

			List<object> toSave = new List<object>();
			toSave.AddRange(dataParser.UsedNomenclatures.Where(x => x.Id == 0));
			toSave.AddRange(dataParser.ChangedEmployees);
			toSave.AddRange(UsedRows.Where(x => x.Operation != null).Select(x => x.Operation));
			return toSave;
		}

		public void MatchAndChanged(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			dataParser.MatchChanges(this, progress, settingsWorkwearItemsViewModel, CountersViewModel, uow, UsedRows);
			OnPropertyChanged(nameof(DisplayRows));

			RecalculateCounters();
			CanSave = CountersViewModel.GetCount(CountersWorkwearItems.NewOperations) > 0;
		}

		public void CleanMatch()
		{
			foreach(var row in UsedRows) {
				row.ChangedColumns.Clear();
				//FIXME Возможно надо чистить сотрудника и операцию, но нужно проверять.
			}
		}

		protected override void RowOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.RowOnPropertyChanged(sender, e);
			RecalculateCounters();
		}

		private void RecalculateCounters()
		{
			CountersViewModel.SetCount(CountersWorkwearItems.SkipRows, UsedRows.Count(x => x.Skipped));
			CountersViewModel.SetCount(CountersWorkwearItems.UsedEmployees, UsedRows.Select(x => x.Employee).Distinct().Count(x => x!= null));
			CountersViewModel.SetCount(CountersWorkwearItems.NewOperations, UsedRows.Count(x => x.Operation != null && x.Operation.Id == 0));
			CountersViewModel.SetCount(CountersWorkwearItems.WorkwearItemNotFound, UsedRows.Count(x => x.Employee != null && x.WorkwearItem == null));
			CountersViewModel.SetCount(CountersWorkwearItems.NewNomenclatures, dataParser.UsedNomenclatures.Count(x => x.Id == 0));
			logger.Debug(String.Join("\n", CountersViewModel.CountersText));
		}
	}
}