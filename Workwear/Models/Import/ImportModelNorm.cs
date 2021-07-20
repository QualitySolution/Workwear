using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using workwear.ViewModels.Import;

namespace workwear.Models.Import
{
	public class ImportModelNorm : ImportModelBase<DataTypeNorm, SheetRowNorm>, IImportModel
	{
		private readonly DataParserNorm dataParser;

		public ImportModelNorm(DataParserNorm dataParser) : base(dataParser)
		{
			this.dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
		}

		#region Параметры
		public string ImportName => "Загрузка норм";

		public string DataColunmsRecomendations => "Установите номер строки с заголовком данных, таким образом чтобы название колонок было корретно. Если в таблице заголовки отутствуют укажите 0.\nДалее для каждой значимой колонки проставьте тип данных которых находится в таблице.\nПри загрузки листа программа автоматически пытается найти залоговок таблицы и выбрать тип данных.\nОбязательными данными являются Должность, Номенклатура нормы, Количество и период";

		public Type CountersEnum => typeof(CountersNorm);

		#endregion

		public override bool CanMatch => (Columns.Any(x => x.DataType == DataTypeNorm.Post)
			&& Columns.Any(x => x.DataType == DataTypeNorm.ProtectionTools)
			&& Columns.Any(x => x.DataType == DataTypeNorm.PeriodAndCount)
		);

		public List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			var rows = UsedRows.Where(x => x.ChangedColumns.Any()).ToList();
			progress.Start(maxValue: rows.Count, text: "Подготовка");

			List<object> toSave = new List<object>();
			foreach(var row in rows) {
				toSave.AddRange(dataParser.PrepareToSave(uow, row));
			}
			return toSave;
		}

		public void MatchAndChanged(IUnitOfWork uow, CountersViewModel counters)
		{
			dataParser.MatchWithExist(uow, UsedRows, Columns);
			dataParser.FindChanges(UsedRows, Columns.Where(x => x.DataType != DataTypeNorm.Unknown).ToArray());
			OnPropertyChanged(nameof(DisplayRows));
		}
	}
}
