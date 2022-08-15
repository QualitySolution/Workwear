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
using workwear.Models.Import.Norms.DataTypes;

namespace workwear.Models.Import.Norms
{
	public class DataParserNorm : DataParserBase
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
			SupportDataTypes.Add( new DataTypeProtectionTools());
			SupportDataTypes.Add( new DataTypePeriodAndCount());
			SupportDataTypes.Add( new DataTypeSubdivision());
			SupportDataTypes.Add( new DataTypePost());

			this.normRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));
			this.protectionToolsRepository = protectionToolsRepository ?? throw new ArgumentNullException(nameof(protectionToolsRepository));
			this.sizeService = sizeService;
		}

		#region Обработка изменений
		public void FindChanges(IEnumerable<SheetRowNorm> list, ExcelValueTarget[] meaningfulColumns)
		{
			foreach(var row in list) {
				foreach(var column in meaningfulColumns) {
					var datatype = (DataTypeNormBase)column.DataType;
					datatype.CalculateChange(row, column);
				}
				if(row.HasChanges)
					row.ToSave.Add(row.NormItem);
			}
		}

		public ChangeState CalculateChange(SheetRowNorm row, DataType dataType, string value) {
			var dataTypeEnum = (DataTypeNorm)dataType.Data;
			if(String.IsNullOrWhiteSpace(value)) {
				return new ChangeState(ChangeType.NotChanged);
			}

			switch(dataTypeEnum) {
				case DataTypeNorm.ProtectionTools:
					

				default:
					throw new NotSupportedException($"Тип данных {dataTypeEnum} не поддерживается.");
			}
		}
		#endregion

		#region Сопоставление данных

		public void MatchWithExist(IUnitOfWork uow, IEnumerable<SheetRowNorm> list, ImportModelNorm model, IProgressBarDisplayable progress)
		{
			progress.Start(10, text: "Сопоставление с существующими данными");
			var postColumn = model.GetColumnForDataType(DataTypeNorm.Post);
			var subdivisionColumn = model.GetColumnForDataType(DataTypeNorm.Subdivision);
			var protectionToolsColumn = model.GetColumnForDataType(DataTypeNorm.ProtectionTools);

			foreach(var row in list) {
				var postValue = row.CellStringValue(postColumn);
				var subdivisionName = subdivisionColumn != null ? row.CellStringValue(subdivisionColumn) : null;

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
				SetOrMakePost(pair, posts, subdivisions, subdivisionColumn == null);
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
			var protectionNames = list.Select(x => x.CellStringValue(protectionToolsColumn)).Where(x => x != null).Distinct().ToArray();
			progress.Add();
			var protections = protectionToolsRepository.GetProtectionToolsByName(uow, protectionNames);
			progress.Add();
			foreach(var row in list.Where(x => !x.Skipped)) {
				if(row.SubdivisionPostCombination.Norms.Count > 1) {
					row.ProgramSkipped = true;
					continue;
				}

				var protectionName = row.CellStringValue(protectionToolsColumn);
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

		void SetOrMakePost(SubdivisionPostCombination combination, IList<Post> posts, IList<Subdivision> subdivisions, bool withoutSubdivision)
		{
			foreach (var postName in combination.PostNames)
			{
				var post = UsedPosts.FirstOrDefault(x =>
							String.Equals(x.Name, postName, StringComparison.CurrentCultureIgnoreCase)
							&& (withoutSubdivision || String.Equals(x.Subdivision?.Name, combination.SubdivisionName, StringComparison.CurrentCultureIgnoreCase)));
				if(post == null) {
					post = posts.FirstOrDefault(x => String.Equals(x.Name, postName, StringComparison.CurrentCultureIgnoreCase)
						&& (withoutSubdivision || String.Equals(x.Subdivision?.Name, combination.SubdivisionName, StringComparison.CurrentCultureIgnoreCase)));

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
		public readonly List<string> UndefinedProtectionNames = new List<string>();
		#endregion

	}
}
