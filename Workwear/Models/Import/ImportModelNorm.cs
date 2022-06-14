using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;

namespace workwear.Models.Import
{
	public class ImportModelNorm : ImportModelBase<DataTypeNorm, SheetRowNorm>, IImportModel
	{
		private readonly DataParserNorm dataParser;

		public ImportModelNorm(DataParserNorm dataParser) : base(dataParser, typeof(CountersNorm))
		{
			this.dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
		}

		#region Параметры
		public string ImportName => "Загрузка норм";

		public string DataColumnsRecommendations => "Установите номер строки с заголовком данных, таким образом чтобы название колонок было корректно. Если в таблице заголовки отсутствуют укажите 0.\nДалее для каждой значимой колонки проставьте тип данных которые находится в таблице.\nПри загрузки листа программа автоматически пытается найти заголовок таблицы и выбрать тип данных.\nОбязательными данными являются Должность, Номенклатура нормы, Количество и период";

		#endregion

		protected override DataTypeNorm[] RequiredDataTypes => new[]
			{ DataTypeNorm.Post, DataTypeNorm.ProtectionTools, DataTypeNorm.PeriodAndCount };

		public bool CanSave { get; private set; }

		public List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			var rows = UsedRows.Where(x => !x.Skipped && x.ChangedColumns.Any()).ToList();
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

		public void MatchAndChanged(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			dataParser.MatchWithExist(uow, UsedRows, Columns);
			dataParser.FindChanges(UsedRows, Columns.Where(x => x.DataType != DataTypeNorm.Unknown).ToArray());
			OnPropertyChanged(nameof(DisplayRows));

			RecalculateCounters();
			CanSave = CountersViewModel.GetCount(CountersNorm.ChangedNormItems) > 0
			          || CountersViewModel.GetCount(CountersNorm.NewNormItems) > 0
			          || CountersViewModel.GetCount(CountersNorm.NewNorms) > 0
			          || CountersViewModel.GetCount(CountersNorm.NewPosts) > 0
			          || CountersViewModel.GetCount(CountersNorm.NewSubdivisions) > 0
			          || CountersViewModel.GetCount(CountersNorm.NewProtectionTools) > 0;
		}
		
		protected override void RowOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        	base.RowOnPropertyChanged(sender, e);
        	RecalculateCounters();
        }
        
        private void RecalculateCounters()
        {
	        CountersViewModel.SetCount(CountersNorm.SkipRows, UsedRows.Count(x => x.Skipped));
	        CountersViewModel.SetCount(CountersNorm.AmbiguousNorms, dataParser.MatchPairs.Count(x => x.Norms.Count > 1));
	        CountersViewModel.SetCount(CountersNorm.NewNorms, dataParser.UsedNorms.Count(x => x.Id == 0));
	        CountersViewModel.SetCount(CountersNorm.NewNormItems, UsedRows.Count(x => !x.Skipped && x.NormItem.Id == 0 && x.ChangedColumns.Any()));
	        CountersViewModel.SetCount(CountersNorm.ChangedNormItems, UsedRows.Count(x => !x.Skipped && x.NormItem.Id > 0 && x.ChangedColumns.Any()));	

	        CountersViewModel.SetCount(CountersNorm.NewPosts, dataParser.UsedPosts.Count(x => x.Id == 0));
	        CountersViewModel.SetCount(CountersNorm.NewSubdivisions, dataParser.UsedSubdivisions.Count(x => x.Id == 0));
	        CountersViewModel.SetCount(CountersNorm.NewProtectionTools, dataParser.UsedProtectionTools.Count(x => x.Id == 0));
	        CountersViewModel.SetCount(CountersNorm.NewItemTypes, dataParser.UsedItemTypes.Count(x => x.Id == 0));
	        CountersViewModel.SetCount(CountersNorm.UndefinedItemTypes, dataParser.UndefinedProtectionNames.Count);
        }
	}
}
