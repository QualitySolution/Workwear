using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Repository.Company;
using workwear.Repository.Regulations;
using Workwear.Domain.Regulations;

namespace workwear.Models.Import
{
	public class DataParserNorm : DataParserBase<DataTypeNorm>
	{
		private readonly NormRepository normRepository;
		private readonly ProtectionToolsRepository protectionToolsRepository;
		private readonly SubdivisionRepository subdivisionRepository;
		private readonly PostRepository postRepository;

		public DataParserNorm(
			NormRepository normRepository, 
			ProtectionToolsRepository protectionToolsRepository, 
			SubdivisionRepository subdivisionRepository, 
			PostRepository postRepository)
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
				"Должность"
				);

			this.normRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));
			this.protectionToolsRepository = protectionToolsRepository ?? throw new ArgumentNullException(nameof(protectionToolsRepository));
			this.subdivisionRepository = subdivisionRepository;
			this.postRepository = postRepository;
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
				if(row.Skiped)
					continue;

				foreach(var column in meaningfulColumns) {
					row.ChangedColumns.Add(column, CalculateChange(row, column.DataType, row.CellValue(column.Index)));
				}
			}
		}

		public ChangeType CalculateChange(SheetRowNorm row, DataTypeNorm dataType, string value)
		{
			if(String.IsNullOrWhiteSpace(value))
				return ChangeType.NotChanged;

			if(row.NormItem == null)
				return ChangeType.NewEntity;

			switch(dataType) {
				case DataTypeNorm.Subdivision:
					return row.SubdivisionPostPair.Post.Subdivision?.Id == 0 ? ChangeType.NewEntity : ChangeType.NotChanged;
				case DataTypeNorm.Post:
					return row.SubdivisionPostPair.Post.Id == 0 ? ChangeType.NewEntity : ChangeType.NotChanged;
				case DataTypeNorm.ProtectionTools:
					return row.NormItem.ProtectionTools.Id == 0 ? ChangeType.NewEntity : ChangeType.NotChanged;
				case DataTypeNorm.PeriodAndCount:
					if(TryParsePeriodAndCount(value, out int amount, out int periods, out NormPeriodType periodType)) {
						return (row.NormItem.Amount == amount && row.NormItem.PeriodCount == periods && row.NormItem.NormPeriod == periodType)
								? ChangeType.NotChanged : ChangeType.ChangeValue;
					}
					else
						return ChangeType.ParseError;
				default:
					throw new NotSupportedException($"Тип данных {dataType} не подерживатся.");
			}
		}
		#endregion

		#region Сопоставление данных

		public void MatchWithExist(IUnitOfWork uow, IEnumerable<SheetRowNorm> list, List<ImportedColumn<DataTypeNorm>> columns)
		{
			var postColumn = columns.FirstOrDefault(x => x.DataType == DataTypeNorm.Post);
			var subdivisionColumn = columns.FirstOrDefault(x => x.DataType == DataTypeNorm.Subdivision);
			var protectionToolsColumn = columns.FirstOrDefault(x => x.DataType == DataTypeNorm.ProtectionTools);

			foreach(var row in list) {
				var postName = row.CellValue(postColumn.Index);
				var subdivisionName = subdivisionColumn != null ? row.CellValue(subdivisionColumn.Index) : null;

				if(String.IsNullOrWhiteSpace(postName)) {
					row.Skiped = true;
					continue;
				}

				var pair = MatchPairs.FirstOrDefault(x => x.PostName == postName && x.SubdivisionName == subdivisionName);
				if(pair == null) {
					pair = new SubdivisionPostPair(postName, subdivisionName);
					MatchPairs.Add(pair);
				}
				row.SubdivisionPostPair = pair;
			}

			var postNames = MatchPairs.Select(x => x.PostName).Distinct().ToArray();
			var posts = uow.Session.QueryOver<Post>()
				.Where(x => x.Name.IsIn(postNames))
				.Fetch(SelectMode.Fetch, x => x.Subdivision)
				.List();

			var subdivisionNames = MatchPairs.Select(x => x.SubdivisionName).Distinct().ToArray();
			var subdivisions = uow.Session.QueryOver<Subdivision>()
				.Where(x => x.Name.IsIn(subdivisionNames))
				.List();

			//Заполняем и создаем отсутствующие должности
			foreach(var pair in MatchPairs)
				SetOrMakePost(pair, posts, subdivisions);

			//Заполняем существующие нормы
			var norms = normRepository.GetNormsForPost(uow, UsedPosts.Where(x => x.Id > 0).ToArray());
			foreach(var norm in norms) {
				foreach(var normPost in norm.Posts) {
					var pair = MatchPairs.FirstOrDefault(x => normPost.IsSame(x.Post));
					if(pair != null)
						pair.Norms.Add(norm);
				}
			}
			//Создаем отсутствующие нормы
			foreach(var pair in MatchPairs) {
				if(pair.Norms.Any())
					continue;

				var norm = new Norm();
				norm.AddPost(pair.Post);
				pair.Norms.Add(norm);
			}

			//Заполняем строки
			var protectionNames = list.Select(x => x.CellValue(protectionToolsColumn.Index)).Distinct().ToArray();
			var protections = protectionToolsRepository.GetProtectionToolsByName(uow, protectionNames);
			foreach(var row in list.Where(x => !x.Skiped)) {
				if(row.SubdivisionPostPair.Norms.Count > 1) {
					row.Skiped = true;
					continue;
				}

				var protectionName = row.CellValue(protectionToolsColumn.Index);
				var protection = UsedProtectionTools.FirstOrDefault(x => String.Equals(x.Name, protectionName, StringComparison.CurrentCultureIgnoreCase));
				if(protection == null) {
					protection = protections.FirstOrDefault(x => String.Equals(x.Name, protectionName, StringComparison.CurrentCultureIgnoreCase));
					if(protection == null) {
						protection = new ProtectionTools {Name = protectionName };
						//FIXME Тут думаю надо заполнять тип.
					}
					UsedProtectionTools.Add(protection);
				}

				var norm = row.SubdivisionPostPair.Norms[0];
				row.NormItem = norm.Items.FirstOrDefault(x => protection.IsSame(x.ProtectionTools));
				if(row.NormItem == null) {
					row.NormItem = norm.AddItem(protection);
				}
			}
		}

		void SetOrMakePost(SubdivisionPostPair pair, IList<Post> posts, IList<Subdivision> subdivisions)
		{
			var post = UsedPosts.FirstOrDefault(x =>
							String.Equals(x.Name, pair.PostName, StringComparison.CurrentCultureIgnoreCase)
							&& String.Equals(x.Subdivision?.Name, pair.SubdivisionName, StringComparison.CurrentCultureIgnoreCase));
			if(post == null) {
				post = posts.FirstOrDefault(x => String.Equals(x.Name, pair.PostName, StringComparison.CurrentCultureIgnoreCase)
					&& String.Equals(x.Subdivision?.Name, pair.SubdivisionName, StringComparison.CurrentCultureIgnoreCase));

				if(post == null) {
					post = new Post { Name = pair.PostName };
					if(!String.IsNullOrEmpty(pair.SubdivisionName)) {
						var subdivision = UsedSubdivisions.FirstOrDefault(x =>
							String.Equals(x.Name, pair.SubdivisionName, StringComparison.CurrentCultureIgnoreCase));

						if(subdivision == null) {
							subdivision = subdivisions.FirstOrDefault(x =>
									String.Equals(x.Name, pair.SubdivisionName, StringComparison.CurrentCultureIgnoreCase));

							if(subdivision == null) {
								subdivision = new Subdivision { Name = pair.SubdivisionName };
							}
							UsedSubdivisions.Add(subdivision);
						}
						post.Subdivision = subdivision;
					}
				}
				UsedPosts.Add(post);
			}
			pair.Post = post;
		}

		#endregion

		#region Сохранение данных
		public readonly List<SubdivisionPostPair> MatchPairs = new List<SubdivisionPostPair>();
		public IEnumerable<Norm> UsedNorms => MatchPairs.Where(x => x.Norms.Count == 1).Select(x => x.Norms[0]);
		public readonly List<Subdivision> UsedSubdivisions = new List<Subdivision>();
		public readonly List<Post> UsedPosts = new List<Post>();
		public readonly List<ProtectionTools> UsedProtectionTools = new List<ProtectionTools>();

		public IEnumerable<object> PrepareToSave(IUnitOfWork uow, SheetRowNorm row)
		{
			//Здесь колонки сортируются чтобы процесс обработки данных был в порядке следования описания типов в Enum
			//Это надо для того чтобы наличие 2 полей с похожими данными заполнялись правильно. Например чтобы отдельное поле с фамилией могло перезаписать значение фамилии поученой из общего поля ФИО.
			foreach(var column in row.ChangedColumns.Keys.OrderBy(x => x.DataType)) {
				SetValue(uow, row.NormItem, column.DataType, row.CellValue(column.Index));
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
					throw new NotSupportedException($"Тип данных {dataType} не подерживатся.");
			}
		}
		#endregion
		#region Helpers
		private bool TryParsePeriodAndCount(string value, out int amount, out int periods, out NormPeriodType periodType)
		{
			amount = 0;
			periods = 0;
			periodType = NormPeriodType.Month;
			var regexp = new Regex(@"\((\d+) в (\d+) (месяц|месяца|месяцев)\)");
			var matches = regexp.Matches(value);
			if(matches.Count == 0)
				return false;
			var parts = matches[0].Groups;
			if(parts.Count != 4)
				return false;
			amount = int.Parse(parts[1].Value);
			periods = int.Parse(parts[2].Value);
			return true;
		}

		#endregion
	}
}
