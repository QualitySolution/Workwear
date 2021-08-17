using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public class ImportModelWorkwearItems : ImportModelBase<DataTypeWorkwearItems, SheetRowWorkwearItems>, IImportModel
	{
		private readonly DataParserWorkwearItems dataParser;

		public ImportModelWorkwearItems(DataParserWorkwearItems dataParser) : base(dataParser)
		{
			this.dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
		}

		#region Параметры
		public string ImportName => "Загрузка выдачи";

		public string DataColunmsRecomendations => "Установите номер строки с заголовком данных, таким образом чтобы название колонок было корректно. Если в таблице заголовки отутствуют укажите 0.\nДалее для каждой значимой колонки проставьте тип данных которые находится в таблице.\nОбязательными данными являются Табельный номер, Номенклатура нормы и выдачи, Дата и количество выдачи";

		public Type CountersEnum => typeof(CountersWorkwearItems);

		#endregion

		public override bool CanMatch => (Columns.Any(x => x.DataType == DataTypeWorkwearItems.PersonnelNumber)
			&& Columns.Any(x => x.DataType == DataTypeWorkwearItems.ProtectionTools)
			&& Columns.Any(x => x.DataType == DataTypeWorkwearItems.Nomenclature)
			&& Columns.Any(x => x.DataType == DataTypeWorkwearItems.Count)
			&& Columns.Any(x => x.DataType == DataTypeWorkwearItems.IssueDate)
		);

		public bool CanSave { get; private set; }

		public List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			var countColumn = Columns.First(x => x.DataType == DataTypeWorkwearItems.Count);
			var rows = UsedRows.Where(x => !x.Skiped && x.ChangedColumns.Any()).ToList();
			var grouped = UsedRows.Where(x => x.Operation != null)
				.GroupBy(x => x.Employee);
			progress.Start(maxValue: grouped.Count(), text: "Подготовка");
			foreach(var employeeGroup in grouped) {
				progress.Add( text: $"Подготовка {employeeGroup.Key.ShortName}");
				var rowByItem = employeeGroup.GroupBy(x => x.WorkwearItem);
				foreach(var itemGroup in rowByItem) {
					var last = itemGroup.OrderByDescending(x => x.Date).First();
					itemGroup.Key.LastIssue = last.Date;
					itemGroup.Key.Amount = last.CellIntValue(countColumn.Index).Value;
					itemGroup.Key.NextIssue = itemGroup.Key.ActiveNormItem.CalculateExpireDate(last.Date.Value, itemGroup.Key.Amount);
					dataParser.ChangedEmployees.Add(employeeGroup.Key);
				}
			}

			List<object> toSave = new List<object>();
			toSave.AddRange(dataParser.UsedNomeclature.Where(x => x.Id == 0));
			toSave.AddRange(dataParser.ChangedEmployees);
			toSave.AddRange(UsedRows.Where(x => x.Operation != null).Select(x => x.Operation));
			return toSave;
		}

		public void MatchAndChanged(IProgressBarDisplayable progress, IUnitOfWork uow, CountersViewModel counters)
		{
			dataParser.MatchChanges(progress, counters, uow, UsedRows, Columns);
			OnPropertyChanged(nameof(DisplayRows));

			counters.SetCount(CountersWorkwearItems.SkipRows, UsedRows.Count(x => x.Skiped));
			counters.SetCount(CountersWorkwearItems.UsedEmployees, UsedRows.Select(x => x.Employee).Distinct().Count(x => x!= null));
			counters.SetCount(CountersWorkwearItems.NewOperations, UsedRows.Count(x => x.Operation != null && x.Operation.Id == 0));
			counters.SetCount(CountersWorkwearItems.WorkwearItemNotFound, UsedRows.Count(x => x.Employee != null && x.WorkwearItem == null));
			counters.SetCount(CountersWorkwearItems.NewNomenclatures, dataParser.UsedNomeclature.Count(x => x.Id == 0));

			CanSave = counters.GetCount(CountersWorkwearItems.NewOperations) > 0;
		}
	}
}
