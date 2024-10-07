using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using Workwear.Repository.Regulations;
using Workwear.Models.Import.Norms.DataTypes;
using Workwear.Tools.Sizes;
using Workwear.ViewModels.Import;

namespace Workwear.Models.Import.Norms
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
			this.normRepository = normRepository ?? throw new ArgumentNullException(nameof(normRepository));
			this.protectionToolsRepository = protectionToolsRepository ?? throw new ArgumentNullException(nameof(protectionToolsRepository));
			this.sizeService = sizeService;
		}

		public void CreateDatatypes(IUnitOfWork uow, SettingsNormsViewModel settings) {
			SupportDataTypes.Add(new DataTypeProtectionTools());
			SupportDataTypes.Add(new DataTypePeriodAndCount(settings));//Должна быть выше колонки с количеством, так как у них одинаковые слова для определения. А вариант с наличием в одной колонке обоих типов данных встречается чаще.
			SupportDataTypes.Add(new DataTypeAmount());
			SupportDataTypes.Add(new DataTypePeriod());
			SupportDataTypes.Add(new DataTypeSubdivision());
			SupportDataTypes.Add(new DataTypeDepartment());
			SupportDataTypes.Add(new DataTypePost());
			SupportDataTypes.Add(new DataTypeParagraph());
			SupportDataTypes.Add(new DataTypeCondition());
			SupportDataTypes.Add(new DataTypeSimpleString(DataTypeNorm.Name, n => n.Name, new []{"название", "наименование"}));
		}

		#region Обработка изменений
		public void FindChanges(IEnumerable<SheetRowNorm> list, ExcelValueTarget[] meaningfulColumns)
		{
			foreach(var row in list) {
				if(row.ProgramSkipped)
					continue;
				foreach(var column in meaningfulColumns.OrderBy(x => x.DataType.ValueSetOrder)) {
					var datatype = (DataTypeNormBase)column.DataType;
					datatype.CalculateChange(row, column);
				}
				if(row.HasChanges)
					row.ToSave.Add(row.NormItem);
			}
		}
		#endregion

		#region Сопоставление данных

		public void MatchWithExist(IUnitOfWork uow, IEnumerable<SheetRowNorm> list, ImportModelNorm model, IProgressBarDisplayable progress)
		{
			progress.Start(10, text: "Сопоставление с существующими данными");
			var postColumn = model.GetColumnForDataType(DataTypeNorm.Post);
			var subdivisionColumn = model.GetColumnForDataType(DataTypeNorm.Subdivision);
			var departmentColumn = model.GetColumnForDataType(DataTypeNorm.Department);
			var protectionToolsColumn = model.GetColumnForDataType(DataTypeNorm.ProtectionTools);
			var conditionColumn = model.GetColumnForDataType(DataTypeNorm.Condition);
			var nameColumn = model.GetColumnForDataType(DataTypeNorm.Name);
			var periodAndCountColumn = model.GetColumnForDataType(DataTypeNorm.PeriodAndCount);
			var periodColumn = model.GetColumnForDataType(DataTypeNorm.Period);

			foreach(var row in list) {
				var postValue = postColumn != null ? row.CellStringValue(postColumn) : null;
				var subdivisionValue = subdivisionColumn != null ? row.CellStringValue(subdivisionColumn) : null;
				var departmentValue = departmentColumn != null ? row.CellStringValue(departmentColumn) : null;
				var nameValue = nameColumn != null ? row.CellStringValue(nameColumn) : null;

				if(String.IsNullOrWhiteSpace(postValue) && String.IsNullOrWhiteSpace(nameValue)) {
					row.ProgramSkipped = true;
					row.ProgramSkippedReason = "Должность и названия нормы отсутствуют. Нет возможности отличить к какой норме относится строка.";
					continue;
				}

				var pair = MatchPairs.FirstOrDefault(x => x.NameValue == nameValue 
				                                          && x.PostValue == postValue 
				                                          && x.SubdivisionValue == subdivisionValue 
				                                          && x.DepartmentValue == departmentValue);
				if(pair == null) {
					pair = new SubdivisionPostCombination(model.SettingsNormsViewModel, nameValue, postValue, subdivisionValue, departmentValue);
					MatchPairs.Add(pair);
				}
				row.SubdivisionPostCombination = pair;
			}
			progress.Add();

			var allPostNames = MatchPairs
				.Where(x => x.PostNames != null)
				.SelectMany(x => x.PostNames).Distinct().ToArray();
			var posts = uow.Session.QueryOver<Post>()
				.Where(x => x.Name.IsIn(allPostNames))
				.Fetch(SelectMode.Fetch, x => x.Subdivision)
				.Fetch(SelectMode.Fetch, x => x.Department)
				.List();
			progress.Add();
			
			var subdivisions = uow.GetAll<Subdivision>().ToList();
			progress.Add();

			var departmentNames = MatchPairs
				.Where(x => x.DepartmentNames != null)
				.SelectMany(x => x.DepartmentNames)
				.Distinct().ToArray();
			var departments = uow.Session.QueryOver<Department>()
				.Where(x => x.Name.IsIn(departmentNames))
				.List();
			progress.Add();
			
			//Заполняем и создаем отсутствующие должности
			foreach(var pair in MatchPairs) {
				if(pair.AllPostNames.Any())
					SetOrMakePost(model.SettingsNormsViewModel, pair, posts, subdivisions, departments, subdivisionColumn == null, departmentColumn == null, model.FileName);
			}
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
					Name = pair.NameValue,
					Comment = "Импортирована из файла " + model.FileName,
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
			if(model.SettingsNormsViewModel.WearoutToName)
				protectionNames = protectionNames.Union( protectionNames.Select(x => x + " (до износа)")).ToArray();
			progress.Add();
			var protections = protectionToolsRepository.GetProtectionToolsByName(uow, protectionNames);
			progress.Add();
			var conditions = uow.GetAll<NormCondition>().ToList();
			progress.Add();
			foreach(var row in list.Where(x => !x.Skipped)) {
				if(row.SubdivisionPostCombination.Norms.Count > 1) {
					row.ProgramSkipped = true;
					row.ProgramSkippedReason = "Найдено более одной нормы. Не определить какую обновлять.";
					continue;
				}

				var protectionName = row.CellStringValue(protectionToolsColumn);
				if(String.IsNullOrWhiteSpace(protectionName)) {
					row.ProgramSkipped = true;
					row.ProgramSkippedReason = "Номенклатура нормы пустая. Не определить какую стоку нормы создавать.";
					continue;
				}

				if(model.SettingsNormsViewModel.WearoutToName && (row.CellStringValue(periodAndCountColumn ?? periodColumn)?.ToLower().Contains("до износа") ?? false))
					protectionName += " (до износа)";

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

				NormCondition condition = null;
				if(conditionColumn != null) {
					var conditionName = row.CellStringValue(conditionColumn);
					if(!string.IsNullOrWhiteSpace(conditionName)) {
						condition = UsedConditions.FirstOrDefault(x =>
							String.Equals(x.Name, conditionName, StringComparison.CurrentCultureIgnoreCase));
						if(condition == null) {
							condition = conditions.FirstOrDefault(x =>
								            String.Equals(x.Name, conditionName, StringComparison.CurrentCultureIgnoreCase))
							            ?? new NormCondition() { Name = conditionName };
							UsedConditions.Add(condition);
						}
					}
				}

				var norm = row.SubdivisionPostCombination.EditingNorm;
				row.NormItem = norm.Items.FirstOrDefault(x => protection.IsSame(x.ProtectionTools));
				if(row.NormItem == null) {
					if(row.SubdivisionPostCombination.WillAddedProtectionTools.Contains(protection)) {
						row.ProgramSkipped = true;
						row.ProgramSkippedReason = "Номенклатура нормы уже добавлена другой строкой.";
						continue;
					}
					row.SubdivisionPostCombination.WillAddedProtectionTools.Add(protection);

					var normItem = new NormItem() {
						Norm = norm,
						ProtectionTools = protection,
						NormCondition = condition
					};
					row.NormItem = normItem;
					row.AddSetValueAction(0, () => norm.Items.Add(normItem));
				}
			}
			progress.Close();
		}

		internal void SetOrMakePost(SettingsNormsViewModel settings,
			SubdivisionPostCombination combination,
			IList<Post> posts,
			IList<Subdivision> subdivisions,
			IList<Department> departments,
			bool withoutSubdivision,
			bool withoutDepartment,
			string importFileName) {
			foreach(var postName in combination.AllPostNames) {
				string[] subdivisionNames = null;
				if(!withoutSubdivision) {
					subdivisionNames = (postName.subdivision ?? String.Empty)
						.Split(new[] { settings.SubdivisionLevelEnable ? settings.SubdivisionLevelSeparator : "" },
							StringSplitOptions.RemoveEmptyEntries)
						.Select(x => x.Trim())
						.ToArray();
					if(settings.SubdivisionLevelEnable && settings.SubdivisionLevelReverse)
						subdivisionNames = subdivisionNames.Reverse().ToArray();
				}

				var existPosts = UsedPosts.Concat(posts).Where(x =>
					String.Equals(x.Name, postName.post, StringComparison.CurrentCultureIgnoreCase)
					&& (withoutSubdivision || EqualsSubdivision(subdivisionNames, x.Subdivision))
					&& (withoutDepartment || String.Equals(x.Department?.Name, postName.department, StringComparison.CurrentCultureIgnoreCase)))
					.ToList();

				if(!existPosts.Any()) {
					var post = new Post {
						Name = postName.post,
						Comments = "Создана при импорте норм из файла " + importFileName,
					};

					Subdivision subdivision = null;
					Department department = null;

					if(!String.IsNullOrEmpty(postName.subdivision)) {
						subdivision = UsedSubdivisions.Concat(subdivisions).FirstOrDefault(x => EqualsSubdivision(subdivisionNames, x));

						if(subdivision == null)
							subdivision = GetOrCreateSubdivision(subdivisionNames, subdivisions, null);
					}

					if(!String.IsNullOrEmpty(postName.department)) {
						department = UsedDepartments.Concat(departments).FirstOrDefault(x => x.Subdivision == subdivision &&
							String.Equals(x.Name, postName.department, StringComparison.CurrentCultureIgnoreCase));

						if(department == null) {
							department = new Department { Name = postName.department, Subdivision = subdivision, Comments = "Создан при импорте норм из файла " + importFileName};
							UsedDepartments.Add(department);
						}
					}

					post.Subdivision = subdivision;
					post.Department = department;
					existPosts.Add(post);
					UsedPosts.AddRange(existPosts);
				}
				combination.Posts.AddRange(existPosts);
			}
		}

		#endregion

		#region Heplers

		bool EqualsSubdivision(string[] names, Subdivision subdivision) {
			if(names.Length == 0 || subdivision == null)
				return false;
			if(!String.Equals(subdivision.Name, names.Last(), StringComparison.CurrentCultureIgnoreCase))
				return false;
			if(names.Length > 1)
				return EqualsSubdivision(names.Take(names.Length - 1).ToArray(), subdivision.ParentSubdivision);
			return subdivision.ParentSubdivision == null;
		}

		internal Subdivision GetOrCreateSubdivision(string[] names, IList<Subdivision> subdivisions, Subdivision parent) {
			var name = names.First();
			var subdivision = UsedSubdivisions.Concat(subdivisions).FirstOrDefault(x => x.ParentSubdivision == parent 
			                                                                            && String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
			if(subdivision == null) {
				subdivision = new Subdivision {
					Name = name, 
					ParentSubdivision = parent
				};
				UsedSubdivisions.Add(subdivision);
			}

			if(names.Length > 1)
				return GetOrCreateSubdivision(names.Skip(1).ToArray(), subdivisions, subdivision);
			else
				return subdivision;
		}
		#endregion

		#region Сохранение данных
		public readonly List<SubdivisionPostCombination> MatchPairs = new List<SubdivisionPostCombination>();
		
		public readonly List<Subdivision> UsedSubdivisions = new List<Subdivision>();
		public readonly List<Department> UsedDepartments = new List<Department>();
		public readonly List<Post> UsedPosts = new List<Post>();
		public readonly List<ProtectionTools> UsedProtectionTools = new List<ProtectionTools>();
		public readonly List<NormCondition> UsedConditions = new List<NormCondition>();
		public readonly List<string> UndefinedProtectionNames = new List<string>();
		#endregion

	}
}
