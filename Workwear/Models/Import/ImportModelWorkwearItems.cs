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
			var rows = UsedRows.Where(x => !x.Skiped && x.ChangedColumns.Any()).ToList();
			progress.Start(maxValue: rows.Count, text: "Подготовка");

			List<object> toSave = new List<object>();
			toSave.AddRange(dataParser.UsedNomeclature.Where(x => x.Id == 0));
			toSave.AddRange(UsedRows.Where(x => x.Operation != null).Select(x => x.Employee).Distinct());
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
			counters.SetCount(CountersWorkwearItems.EmployeeNotFound, UsedRows.Count(x => x.Employee == null));
			counters.SetCount(CountersWorkwearItems.WorkwearItemNotFound, UsedRows.Count(x => x.Employee != null && x.WorkwearItem == null));
			counters.SetCount(CountersWorkwearItems.NewNomenclatures, dataParser.UsedNomeclature.Count(x => x.Id == 0));

			CanSave = counters.GetCount(CountersWorkwearItems.NewOperations) > 0;
		}
	}
}
