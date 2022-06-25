using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Repository.Regulations;
using Workwear.Domain.Regulations;
using Workwear.Measurements;

namespace workwear.Models.Import
{
	public class DataParserNorm : DataParserBase<DataTypeNorm>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly NormRepository normRepository;
		private readonly ProtectionToolsRepository protectionToolsRepository;
		private readonly SizeService sizeService;

		public DataParserNorm(
			NormRepository normRepository, 
			ProtectionToolsRepository protectionToolsRepository,
			SizeService sizeService)
		{
			AddColumnName(DataTypeNorm.ProtectionTools,
				"номенклатура"
				);
			AddColumnName(DataTypeNorm.PeriodAndCount,
				"Норма выдачи"
				);
			AddColumnName(DataTypeNorm.Subdivision,
				"Подразделение"
				);
			AddColumnName(DataTypeNorm.Post,
				"Должность",
				"Должности",
				"профессия",
				"профессии"
				);

			this.normRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));
			this.protectionToolsRepository = protectionToolsRepository ?? throw new ArgumentNullException(nameof(protectionToolsRepository));
			this.sizeService = sizeService;
		}

		private void AddColumnName(DataTypeNorm type, params string[] names)
		{
			foreach(var name in names)
				ColumnNames.Add(name.ToLower(), type);
		}

		#region Обработка изменений
		public void FindChanges(IEnumerable<SheetRowNorm> list, ImportedColumn<DataTypeNorm>[] meaningfulColumns)
		{
			foreach(var row in list) {
				if(row.Skipped)
					continue;

				foreach(var column in meaningfulColumns) {
					row.AddColumnChange(column, CalculateChange(row, column.DataType, row.CellStringValue(column.Index)));
				}
				if(!row.ChangedColumns.Any(x => x.Key.DataType == DataTypeNorm.PeriodAndCount))
					row.ProgramSkipped = true;
				else if(row.ChangedColumns.First(x => x.Key.DataType == DataTypeNorm.PeriodAndCount).Value.ChangeType == ChangeType.ParseError)
					row.ProgramSkipped = true;
			}
		}

		public ChangeType CalculateChange(SheetRowNorm row, DataTypeNorm dataType, string value)
		{
			if(String.IsNullOrWhiteSpace(value)) {
				return dataType == DataTypeNorm.PeriodAndCount ? ChangeType.ParseError : ChangeType.NotChanged;
			}
				
			switch(dataType) {
				case DataTypeNorm.Subdivision:
					return row.SubdivisionPostCombination.Posts.Any(p => p.Subdivision?.Id == 0) ? ChangeType.NewEntity : ChangeType.NotChanged;
				case DataTypeNorm.Post:
					return row.SubdivisionPostCombination.Posts.Any(p => p.Id == 0) ? ChangeType.NewEntity : ChangeType.NotChanged;
				case DataTypeNorm.ProtectionTools:
					return row.NormItem.ProtectionTools.Id == 0 ? ChangeType.NewEntity : ChangeType.NotChanged;
				case DataTypeNorm.PeriodAndCount:
					if(TryParsePeriodAndCount(value, out int amount, out int periods, out NormPeriodType periodType)) {
						return (row.NormItem.Amount == amount && row.NormItem.PeriodCount == periods && row.NormItem.NormPeriod == periodType)
								? ChangeType.NotChanged : (row.NormItem.Id == 0 ? ChangeType.NewEntity : ChangeType.ChangeValue);
					}
					else
						return ChangeType.ParseError;
				default:
					throw new NotSupportedException($"Тип данных {dataType} не поддерживается.");
			}
		}
		#endregion

		#region Сопоставление данных

		public void MatchWithExist(IUnitOfWork uow, IEnumerable<SheetRowNorm> list, List<ImportedColumn<DataTypeNorm>> columns, IProgressBarDisplayable progress)
		{
			progress.Start(10, text: "Сопоставление с существующими данными");
			var postColumn = columns.FirstOrDefault(x => x.DataType == DataTypeNorm.Post);
			var subdivisionColumn = columns.FirstOrDefault(x => x.DataType == DataTypeNorm.Subdivision);
			var protectionToolsColumn = columns.FirstOrDefault(x => x.DataType == DataTypeNorm.ProtectionTools);

			foreach(var row in list) {
				var postValue = row.CellStringValue(postColumn.Index);
				var subdivisionName = subdivisionColumn != null ? row.CellStringValue(subdivisionColumn.Index) : null;

				if(String.IsNullOrWhiteSpace(postValue)) {
					row.ProgramSkipped = true;
					continue;
				}

				var pair = MatchPairs.FirstOrDefault(x => x.PostValue == postValue && x.SubdivisionName == subdivisionName);
				if(pair == null) {
					pair = new SubdivisionPostCombination(postValue, subdivisionName);
					MatchPairs.Add(pair);
				}
				row.SubdivisionPostCombination = pair;
			}
			progress.Add();

			var allPostNames = MatchPairs.SelectMany(x => x.PostNames).Distinct().ToArray();
			var posts = uow.Session.QueryOver<Post>()
				.Where(x => x.Name.IsIn(allPostNames))
				.Fetch(SelectMode.Fetch, x => x.Subdivision)
				.List();
			progress.Add();

			var subdivisionNames = MatchPairs.Select(x => x.SubdivisionName).Distinct().ToArray();
			var subdivisions = uow.Session.QueryOver<Subdivision>()
				.Where(x => x.Name.IsIn(subdivisionNames))
				.List();
			progress.Add();

			//Заполняем и создаем отсутствующие должности
			foreach(var pair in MatchPairs)
				SetOrMakePost(pair, posts, subdivisions);
			progress.Add();

			//Заполняем существующие нормы
			var norms = normRepository.GetNormsForPost(uow, UsedPosts.Where(x => x.Id > 0).ToArray());
			foreach(var norm in norms) {
				foreach(var normPost in norm.Posts) {
					var pair = MatchPairs.FirstOrDefault(x => x.Posts.Any(p => p.IsSame(normPost)));
					if(pair != null)
						pair.Norms.Add(norm);
				}
			}
			progress.Add();
			//Создаем отсутствующие нормы
			foreach(var pair in MatchPairs) {
				if(pair.Norms.Any())
					continue;

				var norm = new Norm {
					Comment = "Импортирована из Excel"
				};
				foreach(var post in pair.Posts) {
					norm.AddPost(post);
				}
				pair.Norms.Add(norm);
			}
			progress.Add();

			//Заполняем строки
			var nomenclatureTypes = new NomenclatureTypes(uow, sizeService, true);
			progress.Add();
			var protectionNames = list.Select(x => x.CellStringValue(protectionToolsColumn.Index)).Where(x => x != null).Distinct().ToArray();
			progress.Add();
			var protections = protectionToolsRepository.GetProtectionToolsByName(uow, protectionNames);
			progress.Add();
			foreach(var row in list.Where(x => !x.Skipped)) {
				if(row.SubdivisionPostCombination.Norms.Count > 1) {
					row.ProgramSkipped = true;
					continue;
				}

				var protectionName = row.CellStringValue(protectionToolsColumn.Index);
				if(String.IsNullOrWhiteSpace(protectionName)) {
					row.ProgramSkipped = true;
					continue;
				}

				var protection = UsedProtectionTools.FirstOrDefault(x => String.Equals(x.Name, protectionName, StringComparison.CurrentCultureIgnoreCase));
				if(protection == null) {
					protection = protections.FirstOrDefault(x => String.Equals(x.Name, protectionName, StringComparison.CurrentCultureIgnoreCase));
					if(protection == null) {
						protection = new ProtectionTools {Name = protectionName };
						protection.Type = nomenclatureTypes.ParseNomenclatureName(protectionName);
						if(protection.Type == null) {
							protection.Type = nomenclatureTypes.GetUnknownType();
							UndefinedProtectionNames.Add(protectionName);
						}
					}
					UsedProtectionTools.Add(protection);
				}

				var norm = row.SubdivisionPostCombination.Norms[0];
				row.NormItem = norm.Items.FirstOrDefault(x => protection.IsSame(x.ProtectionTools));
				if(row.NormItem == null) {
					row.NormItem = norm.AddItem(protection);
					row.NormItem.Amount = 0;
				}
			}
			progress.Close();
		}

		void SetOrMakePost(SubdivisionPostCombination combination, IList<Post> posts, IList<Subdivision> subdivisions)
		{
			foreach (var postName in combination.PostNames)
			{
				var post = UsedPosts.FirstOrDefault(x =>
							String.Equals(x.Name, postName, StringComparison.CurrentCultureIgnoreCase)
							&& String.Equals(x.Subdivision?.Name, combination.SubdivisionName, StringComparison.CurrentCultureIgnoreCase));
				if(post == null) {
					post = posts.FirstOrDefault(x => String.Equals(x.Name, postName, StringComparison.CurrentCultureIgnoreCase)
						&& String.Equals(x.Subdivision?.Name, combination.SubdivisionName, StringComparison.CurrentCultureIgnoreCase));

					if(post == null) {
						post = new Post { 
							Name = postName,
							Comments = "Создана при импорте норм из Excel", 
						};
						if(!String.IsNullOrEmpty(combination.SubdivisionName)) {
							var subdivision = UsedSubdivisions.FirstOrDefault(x =>
								String.Equals(x.Name, combination.SubdivisionName, StringComparison.CurrentCultureIgnoreCase));

							if(subdivision == null) {
								subdivision = subdivisions.FirstOrDefault(x =>
										String.Equals(x.Name, combination.SubdivisionName, StringComparison.CurrentCultureIgnoreCase));

								if(subdivision == null) {
									subdivision = new Subdivision { Name = combination.SubdivisionName };
								}
								UsedSubdivisions.Add(subdivision);
							}
							post.Subdivision = subdivision;
						}
					}
					UsedPosts.Add(post);
				}
				combination.Posts.Add(post);
			}
		}

		#endregion

		#region Сохранение данных
		public readonly List<SubdivisionPostCombination> MatchPairs = new List<SubdivisionPostCombination>();
		public IEnumerable<Norm> UsedNorms => MatchPairs.Where(x => x.Norms.Count == 1).Select(x => x.Norms[0]);
		public readonly List<Subdivision> UsedSubdivisions = new List<Subdivision>();
		public readonly List<Post> UsedPosts = new List<Post>();
		public readonly List<ProtectionTools> UsedProtectionTools = new List<ProtectionTools>();
		public IEnumerable<ItemsType> UsedItemTypes => UsedProtectionTools.Select(x => x.Type).Distinct();

		public List<string> UndefinedProtectionNames = new List<string>();

		public IEnumerable<object> PrepareToSave(IUnitOfWork uow, SheetRowNorm row)
		{
			//Здесь колонки сортируются чтобы процесс обработки данных был в порядке следования описания типов в Enum
			//Это надо для того чтобы наличие 2 полей с похожими данными заполнялись правильно. Например чтобы отдельное поле с фамилией могло перезаписать значение фамилии поученной из общего поля ФИО.
			foreach(var column in row.ChangedColumns.Keys.OrderBy(x => x.DataType)) {
				SetValue(uow, row.NormItem, column.DataType, row.CellStringValue(column.Index));
			}

			yield return row.NormItem;
		}

		private void SetValue(IUnitOfWork uow, NormItem item, DataTypeNorm dataType, string value)
		{
			if(String.IsNullOrWhiteSpace(value))
				return;

			switch(dataType) {
				case DataTypeNorm.Post:
				case DataTypeNorm.Subdivision:
				case DataTypeNorm.ProtectionTools:
					//Ничего не делаем так как уже заполнено в момент идентификации строки.
					break;
				case DataTypeNorm.PeriodAndCount:
					if(TryParsePeriodAndCount(value, out int amount, out int periods, out NormPeriodType periodType)){
						item.Amount = amount;
						item.PeriodCount = periods;
						item.NormPeriod = periodType;
					}
					break;
				default:
					throw new NotSupportedException($"Тип данных {dataType} не поддерживается.");
			}
		}
		#endregion
		#region Helpers
		internal static bool TryParsePeriodAndCount(string value, out int amount, out int periods, out NormPeriodType periodType)
		{
			amount = 0;
			periods = 0;
			periodType = NormPeriodType.Wearout;
			if(value.ToLower().Contains("до износа")) {
				amount = 1;
				periodType = NormPeriodType.Wearout;
				return true;
			}
			if(value.ToLower().Contains("дежурны")) {
				amount = 1;
				periodType = NormPeriodType.Duty;
				return true;
			}

			var regexp = new Regex(@"(\d+) в (\d+) (месяц|месяца|месяцев)");
			var match = regexp.Match(value);
			if(match.Success)
			{
				periodType = NormPeriodType.Month;
				amount = int.Parse(match.Groups[1].Value);
				periods = int.Parse(match.Groups[2].Value);
				return true;
			}
			regexp = new Regex(@"(\d+).* (\d+)([,\.]5)? *(год|года|лет)");
			match = regexp.Match(value);
			if (match.Success)
			{
				periodType = NormPeriodType.Year;
				amount = int.Parse(match.Groups[1].Value);
				periods = int.Parse(match.Groups[2].Value);
				if (match.Groups[3].Value.EndsWith(",5") || match.Groups[3].Value.EndsWith(".5")) {
                	periods = periods * 12 + 6;
					periodType = NormPeriodType.Month;
				}

				return true;
			}
			regexp = new Regex(@"^(\d+) ?(пар|пара|пары|шт\.?|комплекта?)?$");
			match = regexp.Match(value);
			if (match.Success)
			{
				periodType = NormPeriodType.Year;
				amount = int.Parse(match.Groups[1].Value);
				periods = 1;
				return true;
			}
			return false;
		}

		#endregion
	}
}
