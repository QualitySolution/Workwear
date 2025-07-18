using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Gamma.Utilities;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using QS.Project.Domain;
using QS.Utilities.Numeric;
using QS.Utilities.Text;
using Workwear.Domain.Operations;
using Workwear.Domain.Operations.Graph;
using Workwear.Domain.Regulations;
using Workwear.Repository.Operations;
using Workwear.Repository.Regulations;
using Workwear.Tools;

namespace Workwear.Domain.Company
{

	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "карточки сотрудников",
		Nominative = "карточка сотрудника",
		PrepositionalPlural = "карточках сотрудников",
		Genitive = "карточки сотрудника",
		GenitivePlural = "карточек сотрудников"
	)]
	[HistoryTrace]
	public class EmployeeCard: BusinessObjectBase<EmployeeCard>, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		#region Свойства
		public virtual int Id { get; set; }

		private DateTime lastUpdate;
		[Display (Name = "Последнее обновление")]
		public virtual DateTime LastUpdate {
			get => lastUpdate;
			set => SetField(ref lastUpdate, value);
		}
		
		private string cardNumber;
		[StringLength(15)]
		[Display (Name = "Номер карточки")]
		public virtual string CardNumber {
			get => cardNumber;
			set => SetField (ref cardNumber, value);
		}

		private string personnelNumber;
		[StringLength(15)]
		[Display (Name = "Табельный номер")]
		public virtual string PersonnelNumber {
			get => personnelNumber;
			set => SetField (ref personnelNumber, value?.Trim());
		}

		private string name;
		[StringLength(20)]
		[Display (Name = "Имя")]
		public virtual string FirstName {
			get => name;
			set => SetField(ref name, ToTitleCase(value));
		}

		private string lastName;
		[StringLength(20)]
		[Display (Name = "Фамилия")]
		public virtual string LastName {
			get =>lastName; 
			set => SetField(ref lastName, ToTitleCase(value));
		}

		private string patronymic;
		[StringLength(20)]
		[Display (Name = "Отчество")]
		public virtual string Patronymic {
			get => patronymic;
			set => SetField(ref patronymic, ToTitleCase(value));
		}

		private string cardKey;
		[Display(Name = "UID карты доступа")]
		[StringLength(16, ErrorMessage = "Максимальная длинна UID карты 16 символов или 8 байт в шестнадцатеричном виде")]
		public virtual string CardKey {
			get => cardKey;
			set => SetField(ref cardKey, value?.ToUpper());
		}

		private string phoneNumber;
		[Display(Name = "Телефон")]
		[StringLength(16, ErrorMessage = "Максимальная длинна телефона 16 символов")]
		public virtual string PhoneNumber {
			get => phoneNumber;
			set => SetField(ref phoneNumber, value);
		}
		
		private string email;
		[Display(Name = "Электронная почта")]
		public virtual string Email {
			get => email;
			set => SetField(ref email, value);
		}

		private bool lkRegistered;
		[Display(Name = "Зарегистрирован мобильный кабинет?")]
		public virtual bool LkRegistered {
			get => lkRegistered;
			set => SetField(ref lkRegistered, value);
		}

		private Post post;
		[Display (Name = "Должность")]
		public virtual Post Post {
			get => post;
			set => SetField(ref post, value);
		}

		private Leader leader;
		[Display (Name = "Руководитель")]
		public virtual Leader Leader {
			get => leader;
			set => SetField(ref leader, value);
		}

		private DateTime? hireDate;
		[Display (Name = "Дата поступления")]
		public virtual DateTime? HireDate {
			get => hireDate;
			set => SetField(ref hireDate, value);
		}

		private DateTime? changeOfPositionDate;
		[Display(Name = "Дата изменения должности или перевода")]
		public virtual DateTime? ChangeOfPositionDate
		{
			get => changeOfPositionDate;
			set => SetField(ref changeOfPositionDate, value);
		}

		private DateTime? dismissDate;
		[Display (Name = "Дата увольнения")]
		public virtual DateTime? DismissDate {
			get => dismissDate;
			set => SetField(ref dismissDate, value);
		}
		
		DateTime? birthDate;
		[Display (Name = "Дата рождения")]
		public virtual DateTime? BirthDate {
			get => birthDate;
			set => SetField (ref birthDate, value);
		}

		private Sex sex;
		[Display (Name = "Пол")]
		public virtual Sex Sex {
			get => sex;
			set => SetField(ref sex, value);
		}

		private UserBase createdbyUser;
		[Display (Name = "Карточку создал")]
		public virtual UserBase CreatedbyUser {
			get => createdbyUser;
			set => SetField(ref createdbyUser, value);
		}

		private Subdivision subdivision;
		[Display (Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		private Department department;
		[Display(Name = "Отдел")]
		public virtual Department Department {
			get => department;
			set => SetField(ref department, value);
		}

		private byte[] photo;
		[Display (Name = "Фотография")]
		public virtual byte[] Photo {
			get => photo;
			set => SetField (ref photo, value);
		}

		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => comment;
			set => SetField(ref comment, value);
		}

		#endregion
		#region Размеры одежды
		private IObservableList<EmployeeSize> sizes = new ObservableList<EmployeeSize>();
		[Display (Name = "Размеры")]
		public virtual IObservableList<EmployeeSize> Sizes {
			get => sizes;
			set => SetField(ref sizes, value);
		}
		#endregion
		#region Norms
		private IObservableList<Norm> usedNorms = new ObservableList<Norm>();
		[Display (Name = "Примененные нормы")]
		public virtual IObservableList<Norm> UsedNorms {
			get => usedNorms;
			set => SetField(ref usedNorms, value);
		}
		#endregion
		#region Items
		private IObservableList<EmployeeCardItem> workwearItems = new ObservableList<EmployeeCardItem>();
		[Display (Name = "Спецодежда")]
		public virtual IObservableList<EmployeeCardItem> WorkwearItems {
			get => workwearItems;
			set => SetField(ref workwearItems, value);
		}
		#endregion
		#region Vacation
		private IObservableList<EmployeeVacation> vacations = new ObservableList<EmployeeVacation>();
		[Display(Name = "Отпуска")]
		public virtual IObservableList<EmployeeVacation> Vacations {
			get => vacations;
			set => SetField(ref vacations, value);
		}
		#endregion
		
		#region CostCenters
		private IObservableList<EmployeeCostCenter> costCenters = new ObservableList<EmployeeCostCenter>();
		[Display(Name = "Места возникновения затрат")]
		public virtual IObservableList<EmployeeCostCenter> CostCenters {
			get => costCenters;
			set => SetField(ref costCenters, value);
		}
		
		public virtual void AddCostCenter(EmployeeCostCenter employeeCostCenter) {
			if(CostCenters.Any(x => x.CostCenter.Id == employeeCostCenter.CostCenter.Id)) {
				logger.Warn($"МВЗ {employeeCostCenter.CostCenter.Title} уже добавлен. Пропускаем...");
				return;
			}
			CostCenters.Add(employeeCostCenter);
		}
		#endregion		
		#region EmployeeGroups
            private IObservableList<EmployeeGroupItem> employeeGroupItems = new ObservableList<EmployeeGroupItem>();
            [Display(Name = "Группы")]
            public virtual IObservableList<EmployeeGroupItem> EmployeeGroupItems {
                get => employeeGroupItems;
                set => SetField(ref employeeGroupItems, value);
            }

            public virtual void AddEmployeeGroup(EmployeeGroup employeeGroup) {
                if(employeeGroup.Items.Any(x => x.Employee.Id == Id)) {
                    logger.Warn($"Группа №{employeeGroup.Id} \"{employeeGroup.Name}\" уже добавлена. Пропускаем...");
                    return;
                }
                EmployeeGroupItems.Add(employeeGroup.AddEmployee(this));
            }
		#endregion
		
		#region Расчетные
		public virtual string Title => PersonHelper.PersonNameWithInitials (LastName, FirstName, Patronymic);
		public virtual string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();
		public virtual string ShortName => PersonHelper.PersonNameWithInitials (LastName, FirstName, Patronymic);

		private string ToTitleCase(string str){
			if (string.IsNullOrWhiteSpace(str))
				return null;
			
			var ti = CultureInfo.CurrentCulture.TextInfo;
			return ti.ToTitleCase(str.Trim().ToLower());
		}

		#endregion
		#region Фильтрованные коллекции
		public virtual IEnumerable<EmployeeCardItem> GetUnderreceivedItems(BaseParameters baseParameters, DateTime onDate) => 
			WorkwearItems.Where(x => x.CalculateRequiredIssue(baseParameters, onDate) > 0);
		#endregion
		public EmployeeCard () { }
		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext) {
			if (String.IsNullOrEmpty(FirstName) && String.IsNullOrEmpty(LastName) && String.IsNullOrEmpty(Patronymic))
				yield return new ValidationResult (
					"Должно быть заполнено хотя бы одно из следующих полей: Фамилия, Имя, Отчество)", 
					new[] { this.GetPropertyName (o => o.FirstName),
						this.GetPropertyName (o => o.LastName),
						this.GetPropertyName (o => o.Patronymic) });

			if (Sex == Sex.None)
				yield return new ValidationResult (
					"Пол должен быть указан.", 
					new[] { this.GetPropertyName (o => o.Sex) });

			if(!String.IsNullOrEmpty(CardKey) && !System.Text.RegularExpressions.Regex.IsMatch(CardKey, @"\A\b[0-9A-F]+\b\Z"))
				yield return new ValidationResult("UID карты должен быть задан в шестнадцатеричном виде, то есть может содержать только символы 0-9 и A-F.", new[] { nameof(CardKey) });
			if(!String.IsNullOrEmpty(CardKey) && (CardKey.Length % 2 != 0))
				yield return new ValidationResult("UID карты должен быть задан в шестнадцатеричном виде, число символов должно быть кратно двум.", new[] { nameof(CardKey) });

			var phoneValidator = new PhoneValidator(PhoneFormat.RussiaOnlyHyphenated);
			if(!phoneValidator.Validate(PhoneNumber, true))
				yield return new ValidationResult(
					$"Телефон должен быть задан в формате {PhoneFormat.RussiaOnlyHyphenated.GetEnumTitle()}",
					new[] { nameof(PhoneNumber) });
			
			if(!EmailHelper.Validate(Email, true))
				yield return new ValidationResult(
					$"Некорректный формат email адреса",
					new[] { nameof(Email) });

			if(!String.IsNullOrEmpty(PersonnelNumber)) {

				var result = UoW.Session.QueryOver<EmployeeCard>()
					.Where(x => x.PersonnelNumber == PersonnelNumber && x.DismissDate == DismissDate);
				if(Id > 0)
					result.WhereNot(x => x.Id == Id);
				if(result.RowCount()>0)
					yield return new ValidationResult(
						"Табельный номер должен быть уникальным", 
						new[] { this.GetPropertyName(o => o.PersonnelNumber) });
			}

			if(!String.IsNullOrEmpty(CardNumber)) {

				var result = UoW.Session.QueryOver<EmployeeCard>()
					.Where(x => x.CardNumber == CardNumber);
				if(Id > 0)
					result.WhereNot(x => x.Id == Id);
				if(result.RowCount() > 0)
					yield return new ValidationResult(
						$"Номер карточки {CardNumber} должен быть уникальным",
						new[] { this.GetPropertyName(o => o.CardNumber) });
			}
			
			if(NHibernateUtil.IsInitialized(CostCenters) && CostCenters.Any() && CostCenters.Sum(x => x.Percent) != 1m)
				yield return new ValidationResult(
				"Сумма по МВЗ в должна быть равна 100%", 
				new[] { nameof(CostCenters) });
		}

		#endregion
		#region Функции для работы с коллекцией норм
		public virtual void AddUsedNorm(Norm norm) {
			if(norm == null) {
				logger.Warn ("Попытка добавить null вместо нормы! Ай-Ай-Ай!");
				return;
			}
			if(UsedNorms.Any (p => DomainHelper.EqualDomainObjects (p, norm))) {
				logger.Warn ("Такая норма уже добавлена. Пропускаем...");
				return;
			}
			UsedNorms.Add (norm);
			UpdateWorkwearItems ();
		}

		public virtual void AddUsedNorms(IEnumerable<Norm> norms) {
			foreach(var norm in norms) {
				if(UsedNorms.Any(usedNorm => DomainHelper.EqualDomainObjects(usedNorm, norm))) {
					logger.Warn($"Норма {norm.Title} уже добавлена. Пропускаем...");
					continue;
				}
				UsedNorms.Add(norm);
			}
			UpdateWorkwearItems();
		}

		public virtual void RemoveUsedNorm(Norm norm) {
			UsedNorms.Remove (norm);
			UpdateWorkwearItems ();
		}

		public virtual int NormFromPost(IUnitOfWork uow, NormRepository normRepository, Post post = null) {
			var norms = normRepository.GetNormsForPost(uow, post ?? Post);
			int count = 0;
			foreach(var norm in norms)
				if(!norm.Archival) {
					AddUsedNorm(norm);
					count++;
				}
			return count;
		}
		#endregion
		#region Функции для работы с коллекцией потребностей
		/// <summary>
		/// Для работы функции необходимо иметь заполненный UoW.
		/// </summary>
		public virtual void UpdateWorkwearItems() {
			logger.Info("Пересчитываем требования по спецодежде для сотрудника");
			//Проверяем нужно ли добавлять
			var processed = new List<EmployeeCardItem>();
			foreach(var norm in UsedNorms) {
				if(norm.Archival)
					continue;
				foreach (var normItem in norm.Items) {
					if(!normItem.NormCondition?.MatchesForEmployee(this) ?? false) 
						continue;
					var currentItem = WorkwearItems.FirstOrDefault (i => i.ProtectionTools == normItem.ProtectionTools);
					if (currentItem == null) {
						//FIXME Возможно нужно проверять если что-то подходящее уже выдавалось то пересчитывать.
						currentItem = new EmployeeCardItem (this, normItem);
						WorkwearItems.Add (currentItem);
					}
					if(processed.Contains (currentItem)) {
						if (normItem.AmountPerYear > currentItem.ActiveNormItem.AmountPerYear)
							currentItem.ActiveNormItem = normItem;
					}
					else {
						processed.Add (currentItem);
						currentItem.ActiveNormItem = normItem;
					}
				}
			}
			// Удаляем больше ненужные
			var needRemove = WorkwearItems.Where (i => !processed.Contains (i));
			needRemove.ToList ().ForEach (i => WorkwearItems.Remove (i));
			//Обновляем информацию о прошлых выдачах, перед обновлением даты следующей выдачи. Так как могли добавить строчку, у которой таких данных еще нет.
			if(processed.Any())
				FillWearReceivedInfo(new EmployeeIssueRepository(UoW));
			//Обновляем срок следующей выдачи
			foreach(var item in processed) {
				item.UpdateNextIssue(UoW);
			}
			logger.Info("Ok");
		}

		/// <summary>
		/// Обновляет дату следующей выдачи у потребностей по указанному списку номенклатур нормы.
		/// Перед выполнением обязательно вызвать заполнение информации о получениях FillWearReceivedInfo
		/// </summary>
		/// <param name="protectionTools">Список номенклатур нормы потребности в которых надо обновлять.</param>
		public virtual void UpdateNextIssue(params ProtectionTools[] protectionTools) {
			var ids = new HashSet<int>(protectionTools.Select(x => x.Id));
			foreach(var wearItem in WorkwearItems) {
				if(ids.Contains(wearItem.ProtectionTools.Id))
					wearItem.UpdateNextIssue(UoW);
			}
		}

		/// <summary>
		/// Обновляет дату следующей выдачи у всех потребностей.
		/// Перед выполнением обязательно вызвать заполнение информации о получениях FillWearReceivedInfo
		/// </summary>
		[Obsolete("Под удаление, используйте аналогичный механизм из EmployeeIssueModel.")]
		public virtual void UpdateNextIssueAll() {
			foreach(var wearItem in WorkwearItems) {
				wearItem.UpdateNextIssue(UoW);
			}
		}

		/// <summary>
		/// Метод заполняет информацию о получениях с строках потребности в виде графа Graph. И обновляет LastIssue.
		/// </summary>
		public virtual void FillWearReceivedInfo(EmployeeIssueRepository issueRepository) {
			if(Id == 0) {
				// Нет смысла лезть в базу, так как сотрудник еще не сохранен.
				foreach(var item in WorkwearItems) {
					item.Graph = new IssueGraph();
				}
				return;
			}
			FillWearReceivedInfo(issueRepository.AllOperationsForEmployee(this));
		}

		/// <summary>
		/// Метод заполняет информацию о получениях для строк потребности в виде графа Graph. И обновляет LastIssue.
		/// </summary>
		public virtual void FillWearReceivedInfo(IList<EmployeeIssueOperation> operations) {
			var protectionGroups = 
				operations
					.Where(x => x.ProtectionTools != null)
					.GroupBy(x => x.ProtectionTools.Id)
					.ToDictionary(g => g.Key, g => g);
			
			foreach (var item in WorkwearItems) {
				if(protectionGroups.ContainsKey(item.ProtectionTools.Id)) 
					item.Graph = new IssueGraph(protectionGroups[item.ProtectionTools.Id].ToList<IGraphIssueOperation>());
				else 
					item.Graph = new IssueGraph(new List<IGraphIssueOperation>());
			}
		}

		#endregion
		#region Функции работы с отпусками
		public virtual void AddVacation(EmployeeVacation vacation) {
			vacation.Employee = this;
			Vacations.Add(vacation);
		}
		public virtual bool OnVacation(DateTime date) {
			return CurrentVacation(date) != null;
		}
		public virtual EmployeeVacation CurrentVacation(DateTime date) {
			return Vacations.FirstOrDefault(v => v.BeginDate <= date && v.EndDate.AddDays(1) > date);
		}
		public virtual void RecalculateDatesOfIssueOperations(IUnitOfWork uow,
			EmployeeIssueRepository employeeIssueRepository, BaseParameters baseParameters,
			IInteractiveQuestion askUser, EmployeeVacation vacation) {
			RecalculateDatesOfIssueOperations(uow, employeeIssueRepository, baseParameters, askUser, vacation.BeginDate, vacation.EndDate);
		}

		#endregion
		public virtual void RecalculateDatesOfIssueOperations(IUnitOfWork uow,
			EmployeeIssueRepository employeeIssueRepository, BaseParameters baseParameters,
			IInteractiveQuestion askUser, DateTime begin, DateTime end)
		{
			var operations = employeeIssueRepository.AllOperationsForEmployee(this, q => q.Fetch(SelectMode.Fetch, o => o.ProtectionTools), uow);
			var toRecalculate = operations.Where(x => x.IsTouchDates(begin, end)).ToList();
			foreach (var typeGroup in toRecalculate.GroupBy(o => o.ProtectionTools)) {
				var graph = new IssueGraph(operations.Where(x => typeGroup.Key.IsSame(x.ProtectionTools)).ToList<IGraphIssueOperation>());
				foreach (var operation in typeGroup.OrderBy(o => o.OperationTime.Date).ThenBy(o => o.StartOfUse)) {
					operation.RecalculateDatesOfIssueOperation(graph, baseParameters, askUser);
					uow.Save(operation);
				}
			}
			FillWearReceivedInfo(operations);
			UpdateNextIssueAll();
		}
	}
}

