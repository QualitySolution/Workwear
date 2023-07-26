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
			SupportDataTypes.Add( new DataTypeProtectionTools());
			SupportDataTypes.Add( new DataTypePeriodAndCount(settings));//Должна быть выше колонки с количеством, так как у них одинаковые слова для определения. А вариант с наличием в одной колонке обоих типов данных встречается чаще.
			SupportDataTypes.Add( new DataTypeAmount());
			SupportDataTypes.Add( new DataTypePeriod());
			SupportDataTypes.Add( new DataTypeSubdivision());
			SupportDataTypes.Add( new DataTypeDepartment());
			SupportDataTypes.Add( new DataTypePost());
			SupportDataTypes.Add(new DataTypeCondition(uow.GetAll<NormCondition>().ToList()));
			SupportDataTypes.Add(new DataTypeSimpleString(DataTypeNorm.Name, n => n.Name, new []{"название"}));
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
			var protectionToolsColumn = model.GetColumnForDataType(DataTypeNorm.ProtectionTools);
			var departmentColumn = model.GetColumnForDataType(DataTypeNorm.Department);
			var nameColumn = model.GetColumnForDataType(DataTypeNorm.Name);
			var periodAndCountColumn = model.GetColumnForDataType(DataTypeNorm.PeriodAndCount);

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

			var subdivisionNames = MatchPairs
				.Where(x => x.SubdivisionNames != null)
				.SelectMany(x => x.SubdivisionNames)
				.Distinct().ToArray();
			var subdivisions = uow.Session.QueryOver<Subdivision>()
				.Where(x => x.Name.IsIn(subdivisionNames))
				.List();
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
					SetOrMakePost(pair, posts, subdivisions, departments, model, subdivisionColumn == null, departmentColumn == null);
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

				if(model.SettingsNormsViewModel.WearoutToName && (row.CellStringValue(periodAndCountColumn)?.ToLower().Contains("до износа") ?? false))
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
					};
					row.NormItem = normItem;
					row.AddSetValueAction(0, () => norm.Items.Add(normItem));
				}
			}
			progress.Close();
		}

		void SetOrMakePost(SubdivisionPostCombination combination, IList<Post> posts,
			IList<Subdivision> subdivisions,
			IList<Department> departments,
			ImportModelNorm model,
			bool withoutSubdivision,
			bool withoutDepartment) {
			foreach(var postName in combination.AllPostNames) {
				var post = UsedPosts.Concat(posts).FirstOrDefault(x =>
					String.Equals(x.Name, postName.post, StringComparison.CurrentCultureIgnoreCase)
					&& (withoutSubdivision || String.Equals(x.Subdivision?.Name, postName.subdivision, StringComparison.CurrentCultureIgnoreCase))
					&& (withoutDepartment || String.Equals(x.Department?.Name, postName.department, StringComparison.CurrentCultureIgnoreCase)));

				if(post == null) {
					post = new Post {
						Name = postName.post,
						Comments = "Создана при импорте норм из файла " + model.FileName,
					};

					Subdivision subdivision = null;
					Department department = null;

					if(!String.IsNullOrEmpty(postName.subdivision)) {
						subdivision = UsedSubdivisions.Concat(subdivisions).FirstOrDefault(x =>
							String.Equals(x.Name, postName.subdivision, StringComparison.CurrentCultureIgnoreCase));

						if(subdivision == null) {
							subdivision = new Subdivision { Name = postName.subdivision };
							UsedSubdivisions.Add(subdivision);
						}
					}

					if(!String.IsNullOrEmpty(postName.department)) {
						department = UsedDepartments.Concat(departments).FirstOrDefault(x => x.Subdivision == subdivision &&
							String.Equals(x.Name, postName.department, StringComparison.CurrentCultureIgnoreCase));

						if(department == null) {
							department = new Department { Name = postName.department, Subdivision = subdivision, Comments = "Создан при импорте норм из файла " + model.FileName};
							UsedDepartments.Add(department);
						}
					}

					post.Subdivision = subdivision;
					post.Department = department;
					UsedPosts.Add(post);
				}
				combination.Posts.Add(post);
			}
		}

		#endregion

		#region Сохранение данных
		public readonly List<SubdivisionPostCombination> MatchPairs = new List<SubdivisionPostCombination>();
		
		public readonly List<Subdivision> UsedSubdivisions = new List<Subdivision>();
		public readonly List<Department> UsedDepartments = new List<Department>();
		public readonly List<Post> UsedPosts = new List<Post>();
		public readonly List<ProtectionTools> UsedProtectionTools = new List<ProtectionTools>();
		public readonly List<string> UndefinedProtectionNames = new List<string>();
		#endregion

	}
}
