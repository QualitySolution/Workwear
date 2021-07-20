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
				case DataTypeNorm.Post:
					return row.SubdivisionPostPair.Norms.Any() ? ChangeType.NotChanged : ChangeType.NewEntity;
				case DataTypeNorm.ProtectionTools:
					if(row.NormItem != null)
						return ChangeType.NotChanged;
					else
						return ChangeType.NewEntity;
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
			var matchPairs = new List<SubdivisionPostPair>();

			foreach(var row in list) {
				var postName = row.CellValue(postColumn.Index);
				var subdivisionName = subdivisionColumn != null ? row.CellValue(subdivisionColumn.Index) : null;

				var pair = matchPairs.FirstOrDefault(x => x.PostName == postName && x.SubdivisionName == subdivisionName);
				if(pair == null) {
					pair = new SubdivisionPostPair(postName, subdivisionName);
					matchPairs.Add(pair);
				}
				row.SubdivisionPostPair = pair;
			}

			var postNames = matchPairs.Select(x => x.PostName).Distinct().ToArray();
			var posts = uow.Session.QueryOver<Post>()
				.Where(x => x.Name.IsIn(postNames))
				.Fetch(SelectMode.Fetch, x => x.Subdivision)
				.List();

			foreach(var post in posts) {
				var pair = matchPairs.FirstOrDefault(x => x.PostName == post.Name && x.SubdivisionName == post.Subdivision?.Name);
				if(pair != null)
					pair.Post = post;
			}

			var norms = normRepository.GetNormsForPost(uow, posts.ToArray());
			foreach(var norm in norms) {
				foreach(var normPost in norm.Posts) {
					var pair = matchPairs.FirstOrDefault(x => normPost.IsSame(x.Post));
					if(pair != null)
						pair.Norms.Add(norm);
				}
			}

			var protectionNames = list.Select(x => x.CellValue(protectionToolsColumn.Index)).Distinct().ToArray();
			var protections = protectionToolsRepository.GetProtectionToolsByName(uow, protectionNames);
			foreach(var row in list) {
				row.ProtectionTools = protections.FirstOrDefault(x => x.Name == row.CellValue(protectionToolsColumn.Index));
				if(row.SubdivisionPostPair.Norms.Count > 1)
					row.Skiped = true;
				else if(row.SubdivisionPostPair.Norms.Count == 1 && row.ProtectionTools != null) {
					var norm = row.SubdivisionPostPair.Norms[0];
					row.NormItem = norm.Items.FirstOrDefault(x => row.ProtectionTools.IsSame(x.ProtectionTools));
				}
			}
		}

		#endregion

		#region Сохранение данных
		private List<Norm> createdNorms = new List<Norm>();
		private List<Subdivision> createdSubdivisions = new List<Subdivision>();
		private List<Post> createdPosts = new List<Post>();
		private List<ProtectionTools> createdProtectionTools = new List<ProtectionTools>();

		public IEnumerable<object> PrepareToSave(IUnitOfWork uow, SheetRowNorm row)
		{
			var norm = row.SubdivisionPostPair.Norms.FirstOrDefault();
			if(norm == null) {
				norm = new Norm();
				row.SubdivisionPostPair.Norms.Add(norm);
				if(row.SubdivisionPostPair.Post == null) {
					row.SubdivisionPostPair.Post = createdPosts.FirstOrDefault(x =>
							String.Equals(x.Name, row.SubdivisionPostPair.PostName, StringComparison.CurrentCultureIgnoreCase)
							&& String.Equals(x.Subdivision?.Name, row.SubdivisionPostPair.SubdivisionName, StringComparison.CurrentCultureIgnoreCase));
					if(row.SubdivisionPostPair.Post == null) {
						row.SubdivisionPostPair.Post = new Post { Name = row.SubdivisionPostPair.PostName };
						if(!String.IsNullOrEmpty(row.SubdivisionPostPair.SubdivisionName)){
							row.SubdivisionPostPair.Post.Subdivision = createdSubdivisions.FirstOrDefault(x =>
							String.Equals(x.Name, row.SubdivisionPostPair.SubdivisionName, StringComparison.CurrentCultureIgnoreCase));
							if(row.SubdivisionPostPair.Post.Subdivision == null) {
								row.SubdivisionPostPair.Post.Subdivision = new Subdivision { Name = row.SubdivisionPostPair.SubdivisionName };
								createdSubdivisions.Add(row.SubdivisionPostPair.Post.Subdivision);
								yield return row.SubdivisionPostPair.Post.Subdivision;
							}
						}
						createdPosts.Add(row.SubdivisionPostPair.Post);
						norm.AddPost(row.SubdivisionPostPair.Post);
						yield return row.SubdivisionPostPair.Post;
					}
				}
				yield return norm;
			}
			if(row.ProtectionTools == null) {
				var protectionColumn = row.ChangedColumns.Keys.First(x => x.DataType == DataTypeNorm.ProtectionTools);
				row.ProtectionTools = createdProtectionTools.FirstOrDefault(x =>
							String.Equals(x.Name, row.CellValue(protectionColumn.Index), StringComparison.CurrentCultureIgnoreCase));
				if(row.ProtectionTools == null) {
					row.ProtectionTools = new ProtectionTools { Name = row.CellValue(protectionColumn.Index) };
					//FIXME Тут думаю надо заполнять тип.
					createdProtectionTools.Add(row.ProtectionTools);
					yield return row.ProtectionTools;
				}
			}

			if(row.NormItem == null) {
				row.NormItem = norm.AddItem(row.ProtectionTools);
			}

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
			var parts = regexp.Matches(value);
			if(parts.Count != 3)
				return false;
			amount = int.Parse(parts[0].Value);
			periods = int.Parse(parts[1].Value);
			return true;
		}

		#endregion
	}
}
