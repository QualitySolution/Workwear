using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.Models.Import.Norms
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

		protected override bool HasRequiredDataTypes(IEnumerable<DataTypeNorm> dataTypes) {
			return base.HasRequiredDataTypes(dataTypes) 
			       && (dataTypes.Contains(DataTypeNorm.PeriodAndCount)
			       || (dataTypes.Contains(DataTypeNorm.Period) && dataTypes.Contains(DataTypeNorm.Amount)));
		}

		protected override DataTypeNorm[] RequiredDataTypes => new[]
			{ DataTypeNorm.Post, DataTypeNorm.ProtectionTools };

		public override void Init(IUnitOfWork uow) {
			dataParser.CreateDatatypes(uow);
		}
		
		public bool CanSave { get; private set; }

		public List<object> MakeToSave(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			var rows = UsedRows.Where(x => x.HasChanges).ToList();
			progress.Start(maxValue: rows.Count, text: "Подготовка");

			List<object> toSave = new List<object>();
			toSave.AddRange(SavedSubdivisions);
			toSave.AddRange(SavedPosts);
			toSave.AddRange(SavedItemsTypes);
			toSave.AddRange(SavedProtectionTools);
			toSave.AddRange(SavedNorms);
			foreach(var row in rows) {
				toSave.AddRange(row.PrepareToSave());
			}
			return toSave;
		}
		
		public void MatchAndChanged(IProgressBarDisplayable progress, IUnitOfWork uow)
		{
			dataParser.MatchWithExist(uow, UsedRows, this, progress);
			dataParser.FindChanges(UsedRows, ImportedDataTypes.ToArray());
			OnPropertyChanged(nameof(DisplayRows));

			RecalculateCounters();
			CanSave = CountersViewModel.GetCount(CountersNorm.ChangedNormItems) > 0
			          || CountersViewModel.GetCount(CountersNorm.NewNormItems) > 0
			          || CountersViewModel.GetCount(CountersNorm.NewNorms) > 0
			          || CountersViewModel.GetCount(CountersNorm.NewPosts) > 0
			          || CountersViewModel.GetCount(CountersNorm.NewSubdivisions) > 0
			          || CountersViewModel.GetCount(CountersNorm.NewProtectionTools) > 0;
		}

		public void CleanMatch()
		{
			foreach(var row in UsedRows) {
				row.ChangedColumns.Clear();
				//FIXME Возможно нужно очищать строку, но нужно проверять, на реальных данных
			}
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
	        CountersViewModel.SetCount(CountersNorm.NewNorms, SavedNorms.Count());
	        CountersViewModel.SetCount(CountersNorm.NewNormItems, UsedRows.Count(x => x.HasChanges && x.NormItem.Id == 0));
	        CountersViewModel.SetCount(CountersNorm.ChangedNormItems, UsedRows.Count(x => x.HasChanges && x.NormItem.Id > 0));	

	        CountersViewModel.SetCount(CountersNorm.NewPosts, SavedPosts.Count());
	        CountersViewModel.SetCount(CountersNorm.NewSubdivisions, SavedSubdivisions.Count());
	        CountersViewModel.SetCount(CountersNorm.NewProtectionTools, SavedProtectionTools.Count());
	        CountersViewModel.SetCount(CountersNorm.NewItemTypes, SavedItemsTypes.Count());
	        CountersViewModel.SetCount(CountersNorm.UndefinedItemTypes, dataParser.UndefinedProtectionNames.Count);
        }
        
        #region Справочники

        private IEnumerable<Norm> SavedNorms => UsedRows
	        .Where(x => x.HasChanges)
	        .Select(x => x.NormItem.Norm)
	        .Distinct()
	        .Where(x => x.Id == 0);
        
        private IEnumerable<Post> SavedPosts => UsedRows
	        .Where(x => !x.Skipped)
	        .Select(x => x.NormItem.Norm)
	        .Distinct()
	        .SelectMany(x => x.Posts)
	        .Where(x => x.Id == 0)
	        .Distinct();
        
        private IEnumerable<Subdivision> SavedSubdivisions => UsedRows
	        .Where(x => !x.Skipped)
	        .Select(x => x.NormItem.Norm)
	        .Distinct() 
	        .SelectMany(x => x.Posts)
	        .Distinct()
	        .Select(x => x.Subdivision)
	        .Where(x => x != null)
	        .SelectMany(x => x.AllParents.Union(new []{x}))
	        .Where(x => x?.Id == 0)
	        .Distinct(); 

        private IEnumerable<ProtectionTools> SavedProtectionTools => UsedRows
	        .Where(x => !x.Skipped)
	        .Select(x => x.NormItem.ProtectionTools)
	        .Where(x => x?.Id == 0)
	        .Distinct();

        private IEnumerable<ItemsType> SavedItemsTypes => UsedRows
	        .Where(x => !x.Skipped)
	        .Select(x => x.NormItem.ProtectionTools?.Type)
	        .Where(x => x?.Id == 0)
	        .Distinct();
		
        #endregion
	}
}
