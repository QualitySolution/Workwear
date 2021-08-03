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

		public bool CanSave { get; private set; }

		public List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			var rows = UsedRows.Where(x => !x.Skiped && x.ChangedColumns.Any()).ToList();
			progress.Start(maxValue: rows.Count, text: "Подготовка");

			List<object> toSave = new List<object>();
			toSave.AddRange(dataParser.UsedSubdivisions.Where(x => x.Id == 0));
			toSave.AddRange(dataParser.UsedPosts.Where(x => x.Id == 0));
			toSave.AddRange(dataParser.UsedItemTypes.Where(x => x.Id == 0));
			toSave.AddRange(dataParser.UsedProtectionTools.Where(x => x.Id == 0));
			toSave.AddRange(dataParser.UsedNorms.Where(x => x.Id == 0));
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

			counters.SetCount(CountersNorm.SkipRows, UsedRows.Count(x => x.Skiped));
			counters.SetCount(CountersNorm.AmbiguousNorms, dataParser.MatchPairs.Count(x => x.Norms.Count > 1));
			counters.SetCount(CountersNorm.NewNorms, dataParser.UsedNorms.Count(x => x.Id == 0));
			counters.SetCount(CountersNorm.NewNormItems, UsedRows.Count(x => !x.Skiped && x.NormItem.Id == 0 && x.ChangedColumns.Any()));
			counters.SetCount(CountersNorm.ChangedNormItems, UsedRows.Count(x => !x.Skiped && x.NormItem.Id > 0 && x.ChangedColumns.Any()));	

			counters.SetCount(CountersNorm.NewPosts, dataParser.UsedPosts.Count(x => x.Id == 0));
			counters.SetCount(CountersNorm.NewSubdivisions, dataParser.UsedSubdivisions.Count(x => x.Id == 0));
			counters.SetCount(CountersNorm.NewProtectionTools, dataParser.UsedProtectionTools.Count(x => x.Id == 0));
			counters.SetCount(CountersNorm.NewItemTypes, dataParser.UsedItemTypes.Count(x => x.Id == 0));
			counters.SetCount(CountersNorm.UndefinedItemTypes, dataParser.UndefinedProtectionNames.Count);

			CanSave = counters.GetCount(CountersNorm.ChangedNormItems) > 0
				|| counters.GetCount(CountersNorm.NewNormItems) > 0
				|| counters.GetCount(CountersNorm.NewNorms) > 0
				|| counters.GetCount(CountersNorm.NewPosts) > 0
				|| counters.GetCount(CountersNorm.NewSubdivisions) > 0
				|| counters.GetCount(CountersNorm.NewProtectionTools) > 0;
		}
	}
}
