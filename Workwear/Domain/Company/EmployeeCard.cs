using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Globalization;
using System.Linq;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Domain;
using QS.Utilities.Numeric;
using QS.Utilities.Text;
using workwear.Domain.Operations.Graph;
using workwear.Domain.Regulations;
using workwear.Domain.Stock;
using workwear.Measurements;
using workwear.Repository.Operations;
using workwear.Repository.Regulations;
using workwear.Repository.Stock;
using workwear.Tools;
using Workwear.Domain.Company;
using Workwear.Measurements;

namespace workwear.Domain.Company
{

	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "карточки сотрудников",
		Nominative = "карточка сотрудника",
		PrepositionalPlural = "карточках сотрудников",
		Genitive = "карточки сотрудника"
	)]
	[HistoryTrace]
	public class EmployeeCard: BusinessObjectBase<EmployeeCard>, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		public virtual int Id { get; set; }

		string cardNumber;

		[StringLength(15)]
		[Display (Name = "Номер карточки")]
		public virtual string CardNumber {
			get { return cardNumber; }
			set { SetField (ref cardNumber, value, () => CardNumber); }
		}

		string personnelNumber;

		[StringLength(15)]
		[Display (Name = "Табельный номер")]
		public virtual string PersonnelNumber {
			get { return personnelNumber; }
			set { SetField (ref personnelNumber, value?.Trim()); }
		}

		string name;

		[StringLength(20)]
		[Display (Name = "Имя")]
		public virtual string FirstName {
			get => name;
			set => SetField(ref name, ToTitleCase(value));
		}

		string lastName;

		[StringLength(20)]
		[Display (Name = "Фамилия")]
		public virtual string LastName {
			get =>lastName; 
			set => SetField(ref lastName, ToTitleCase(value));
		}

		string patronymic;

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

		private bool lkRegistered;
		[Display(Name = "Зарегистрирован мобильный кабинет?")]
		public virtual bool LkRegistered {
			get => lkRegistered;
			set => SetField(ref lkRegistered, value);
		}

		Post post;

		[Display (Name = "Должность")]
		public virtual Post Post {
			get { return post; }
			set { SetField (ref post, value, () => Post); }
		}

		Leader leader;

		[Display (Name = "Руководитель")]
		public virtual Leader Leader {
			get { return leader; }
			set { SetField (ref leader, value, () => Leader); }
		}

		DateTime? hireDate;

		[Display (Name = "Дата поступления")]
		public virtual DateTime? HireDate {
			get { return hireDate; }
			set { SetField (ref hireDate, value, () => HireDate); }
		}

		private DateTime? changeOfPositionDate;

		[Display(Name = "Дата изменения должности или перевода")]
		public virtual DateTime? ChangeOfPositionDate
		{
			get { return changeOfPositionDate; }
			set { SetField(ref changeOfPositionDate, value, () => ChangeOfPositionDate); }
		}

		DateTime? dismissDate;

		[Display (Name = "Дата увольнения")]
		public virtual DateTime? DismissDate {
			get { return dismissDate; }
			set { SetField (ref dismissDate, value, () => DismissDate); }
		}
		
		DateTime? birthDate;
		[Display (Name = "Дата рождения")]
		public virtual DateTime? BirthDate {
			get => birthDate;
			set => SetField (ref birthDate, value);
		}

		Sex sex;

		[Display (Name = "Пол")]
		public virtual Sex Sex {
			get { return sex; }
			set { SetField (ref sex, value, () => Sex); }
		}

		UserBase createdbyUser;

		[Display (Name = "Карточку создал")]
		public virtual UserBase CreatedbyUser {
			get { return createdbyUser; }
			set { SetField (ref createdbyUser, value, () => CreatedbyUser); }
		}
			
		Subdivision subdivision;

		[Display (Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get { return subdivision; }
			set { SetField (ref subdivision, value, () => Subdivision); }
		}

		private Department department;
		[Display(Name = "Отдел")]
		public virtual Department Department {
			get => department;
			set => SetField(ref department, value);
		}

		byte[] photo;

		[Display (Name = "Фотография")]
		public virtual byte[] Photo {
			get { return photo; }
			set { SetField (ref photo, value, () => Photo); }
		}

		private string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		#endregion

		#region Размеры одежды

		string wearGrowth;

		[StringLength(10)]
		[Display (Name = "Рост одежды")]
		public virtual string WearGrowth { 
			get { return wearGrowth; } 
			set	{ SetField (ref wearGrowth, value, () => WearGrowth); }
		}

		string wearSizeStd;

		[Display (Name = "Стандарт размера одежды")]
		public virtual string WearSizeStd {
			get { return wearSizeStd; }
			set { SetField (ref wearSizeStd, value, () => WearSizeStd); }
		}

		string wearSize;

		[Display (Name = "Размер одежды")]
		public virtual string WearSize { 
			get { return wearSize; } 
			set	{ SetField (ref wearSize, value, () => WearSize); }
		}

		string shoesSizeStd;

		[Display (Name = "Стандарт размера обуви")]
		public virtual string ShoesSizeStd {
			get { return shoesSizeStd; }
			set { SetField (ref shoesSizeStd, value, () => ShoesSizeStd); }
		}

		string shoesSize;

		[Display (Name = "Размер обуви")]
		public virtual string ShoesSize { 
			get { return shoesSize; } 
			set	{ SetField (ref shoesSize, value, () => ShoesSize); }
		}

		string winterShoesSizeStd;

		[Display(Name = "Стандарт размера зимней обуви")]
		public virtual string WinterShoesSizeStd
		{
			get { return winterShoesSizeStd; }
			set { SetField(ref winterShoesSizeStd, value, () => WinterShoesSizeStd); }
		}

		string winterShoesSize;

		[Display(Name = "Размер зимней обуви")]
		public virtual string WinterShoesSize
		{
			get { return winterShoesSize; }
			set { SetField(ref winterShoesSize, value, () => WinterShoesSize); }
		}

		string headdressSizeStd;

		[Display (Name = "Стандарт размера головного убора")]
		public virtual string HeaddressSizeStd {
			get { return headdressSizeStd; }
			set { SetField (ref headdressSizeStd, value, () => HeaddressSizeStd); }
		}

		string headdressSize;

		[Display (Name = "Размер головного убора")]
		public virtual string HeaddressSize { 
			get { return headdressSize; } 
			set	{ SetField (ref headdressSize, value, () => HeaddressSize); }
		}

		string glovesSizeStd;

		[Display (Name = "Стандарт размера перчаток")]
		public virtual string GlovesSizeStd {
			get { return glovesSizeStd; }
			set { SetField (ref glovesSizeStd, value, () => GlovesSizeStd); }
		}

		string glovesSize;

		[Display (Name = "Размер перчаток")]
		public virtual string GlovesSize { 
			get { return glovesSize; } 
			set	{ SetField (ref glovesSize, value, () => GlovesSize); }
		}

		public virtual string MittensSizeStd => SizeHelper.GetSizeStdCode(SizeStandartMittens.Rus);

		string mittensSize;

		[Display(Name = "Размер рукавиц")]
		public virtual string MittensSize {
			get { return mittensSize; }
			set { SetField(ref mittensSize, value); }
		}

		private IList<Norm> usedNorms = new List<Norm>();

		[Display (Name = "Примененные нормы")]
		public virtual IList<Norm> UsedNorms {
			get { return usedNorms; }
			set { SetField (ref usedNorms, value, () => UsedNorms); }
		}

		GenericObservableList<Norm> observableUsedNorms;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Norm> ObservableUsedNorms {
			get {
				if (observableUsedNorms == null)
					observableUsedNorms = new GenericObservableList<Norm> (UsedNorms);
				return observableUsedNorms;
			}
		}

		private IList<EmployeeCardItem> workwearItems = new List<EmployeeCardItem>();

		[Display (Name = "Спецодежда")]
		public virtual IList<EmployeeCardItem> WorkwearItems {
			get { return workwearItems; }
			set { SetField (ref workwearItems, value, () => WorkwearItems); }
		}

		GenericObservableList<EmployeeCardItem> observableWorkwearItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<EmployeeCardItem> ObservableWorkwearItems {
			get {
				if (observableWorkwearItems == null)
					observableWorkwearItems = new GenericObservableList<EmployeeCardItem> (WorkwearItems);
				return observableWorkwearItems;
			}
		}

		private IList<EmployeeVacation> vacations = new List<EmployeeVacation>();

		[Display(Name = "Отпуска")]
		public virtual IList<EmployeeVacation> Vacations {
			get { return vacations; }
			set { SetField(ref vacations, value, () => Vacations); }
		}

		GenericObservableList<EmployeeVacation> observableVacations;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<EmployeeVacation> ObservableVacations {
			get {
				if(observableVacations == null)
					observableVacations = new GenericObservableList<EmployeeVacation>(Vacations);
				return observableVacations;
			}
		}

		#endregion

		#region Расчетные

		public virtual string Title {
			get{ return PersonHelper.PersonNameWithInitials (LastName, FirstName, Patronymic);
			}
		}

		public virtual string FullName {
			get { return String.Format ("{0} {1} {2}", LastName, FirstName, Patronymic).Trim (); }
		}

		public virtual string ShortName {
			get { return PersonHelper.PersonNameWithInitials (LastName, FirstName, Patronymic); }
		}

		private string ToTitleCase(string str){
			if(!string.IsNullOrWhiteSpace(str)) {
				TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
				return ti.ToTitleCase(str.Trim().ToLower());
			}
			return string.Empty;
		}

		#endregion

		#region Фильтрованные коллекции

		public virtual IEnumerable<EmployeeCardItem> GetUnderreceivedItems(BaseParameters baseParameters) => WorkwearItems.Where(x => x.CalculateRequiredIssue(baseParameters) > 0);

		#endregion

		public EmployeeCard ()
		{
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (String.IsNullOrEmpty (FirstName) && String.IsNullOrEmpty (LastName) && String.IsNullOrEmpty (Patronymic))
				yield return new ValidationResult ("Должно быть заполнено хотя бы одно из следующих полей: " +
					"Фамилия, Имя, Отчество)", 
					new[] { this.GetPropertyName (o => o.FirstName), this.GetPropertyName (o => o.LastName), this.GetPropertyName (o => o.Patronymic) });

			if (Sex == Sex.None)
				yield return new ValidationResult ("Пол должен быть указан.", new[] { this.GetPropertyName (o => o.Sex) });

			if(!String.IsNullOrEmpty(CardKey) && !System.Text.RegularExpressions.Regex.IsMatch(CardKey, @"\A\b[0-9A-F]+\b\Z"))
				yield return new ValidationResult("UID карты должен быть задан в шестнадцатеричном виде, то есть может содержать только символы 0-9 и A-F.", new[] { nameof(CardKey) });
			if(!String.IsNullOrEmpty(CardKey) && (CardKey.Length % 2 != 0))
				yield return new ValidationResult("UID карты должен быть задан в шестнадцатеричном виде, число символов должно быть кратно двум.", new[] { nameof(CardKey) });

			var phoneValidator = new PhoneValidator(PhoneFormat.RussiaOnlyHyphenated);
			if(!phoneValidator.Validate(PhoneNumber, true))
				yield return new ValidationResult($"Телефон должен быть задан в формате {PhoneFormat.RussiaOnlyHyphenated.GetEnumTitle()}", new[] { nameof(PhoneNumber) });

			if(!String.IsNullOrEmpty(PersonnelNumber)) {

				var result = UoW.Session.QueryOver<EmployeeCard>()
					.Where(x => x.PersonnelNumber == PersonnelNumber && x.DismissDate == DismissDate);
				if(Id > 0)
					result.WhereNot(x => x.Id == Id);
				if(result.RowCount()>0)
					yield return new ValidationResult("Табельный номер должен быть уникальным", new[] { this.GetPropertyName(o => o.PersonnelNumber) });
			}

			if(!String.IsNullOrEmpty(CardNumber)) {

				var result = UoW.Session.QueryOver<EmployeeCard>()
					.Where(x => x.CardNumber == CardNumber);
				if(Id > 0)
					result.WhereNot(x => x.Id == Id);
				if(result.RowCount() > 0)
					yield return new ValidationResult($"Номер карточки {CardNumber} должен быть уникальным", new[] { this.GetPropertyName(o => o.CardNumber) });
			}
		}

		#endregion

		#region Работа с размерами
		public virtual SizePair GetSize(СlothesType wearCategory)
		{
			switch (wearCategory)
			{
				case СlothesType.Wear:
					return new SizePair(WearSizeStd, WearSize);
				case СlothesType.Shoes:
					return new SizePair(ShoesSizeStd, ShoesSize);
				case СlothesType.WinterShoes:
					return new SizePair(WinterShoesSizeStd, WinterShoesSize);
				case СlothesType.Gloves:
					return new SizePair(GlovesSizeStd, GlovesSize);
				case СlothesType.Mittens:
					return new SizePair(SizeHelper.GetSizeStdCode(SizeStandartMittens.Rus), MittensSize);
				case СlothesType.Headgear:
					return new SizePair(HeaddressSizeStd, HeaddressSize);
				default:
					return null;
			}
		}

		public virtual void SetSize(СlothesType wearCategory, SizePair size)
		{
			switch(wearCategory) {
				case СlothesType.Wear:
					WearSizeStd = size.StandardCode;
					WearSize = size.Size;
					break;
				case СlothesType.Shoes:
					ShoesSizeStd = size.StandardCode;
					ShoesSize = size.Size;
					break;
				case СlothesType.WinterShoes:
					WinterShoesSizeStd = size.StandardCode;
					WinterShoesSize = size.Size;
					break;
				case СlothesType.Gloves:
					GlovesSizeStd = size.StandardCode;
					GlovesSize = size.Size;
					break;
				case СlothesType.Mittens:
					if(size.StandardCode != SizeHelper.GetSizeStdCode(SizeStandartMittens.Rus))
						throw new NotImplementedException("Другие стандарты не реализованы");
					MittensSize = size.Size;
					break;
				case СlothesType.Headgear:
					HeaddressSizeStd = size.StandardCode;
					HeaddressSize = size.Size;
					break;
				default:
					throw new NotImplementedException($"Работам с типом одежды {wearCategory} пока не реализована");
			}
		}

		#endregion

		#region Функции для работы с коллекцией норм

		public virtual void AddUsedNorm(Norm norm)
		{
			if(UsedNorms.Any (p => DomainHelper.EqualDomainObjects (p, norm)))
			{
				logger.Warn ("Такая норма уже добавлена. Пропускаем...");
				return;
			}
			ObservableUsedNorms.Add (norm);
			UpdateWorkwearItems ();
		}

		public virtual void RemoveUsedNorm(Norm norm)
		{
			ObservableUsedNorms.Remove (norm);
			UpdateWorkwearItems ();
		}

		public virtual void NormFromPost(IUnitOfWork uow, NormRepository normRepository)
		{
			var norms = normRepository.GetNormsForPost(UoW, Post);
			foreach(var norm in norms)
				AddUsedNorm(norm);
		}

		#endregion

		#region Функции для работы с коллекцией потребностей

		/// <summary>
		/// Для работы функции необходимо иметь заполненный UoW.
		/// </summary>
		public virtual void UpdateWorkwearItems()
		{
			logger.Info("Пересчитываем требования по спецодежде для сотрудника");
			//Проверяем нужно ли добавлять
			var processed = new List<EmployeeCardItem>();
			foreach(var norm in UsedNorms)
			{
				foreach (var normItem in norm.Items)
				{

					if(!normItem.NormCondition?.MatchesForEmployee(this) ?? false) 
						continue;

					var currentItem = WorkwearItems.FirstOrDefault (i => i.ProtectionTools == normItem.ProtectionTools);

					if (currentItem == null)
					{
						//FIXME Возможно нужно проверять если что-то подходящее уже выдавалось то пересчитывать.
						currentItem = new EmployeeCardItem (this, normItem);
						ObservableWorkwearItems.Add (currentItem);
					}

					if(processed.Contains (currentItem))
					{
						if (normItem.AmountPerYear > currentItem.ActiveNormItem.AmountPerYear)
							currentItem.ActiveNormItem = normItem;
					}
					else
					{
						processed.Add (currentItem);
						currentItem.ActiveNormItem = normItem;
					}
				}
			}
			// Удаляем больше ненужные
			var needRemove = WorkwearItems.Where (i => !processed.Contains (i));

			needRemove.ToList ().ForEach (i => ObservableWorkwearItems.Remove (i));
			//Обновляем срок следующей выдачи
			foreach(var item in processed)
			{
				item.UpdateNextIssue(UoW);
			}
			logger.Info("Ok");
		}

		public virtual void UpdateNextIssue(params ProtectionTools[] protectionTools)
		{
			var ids = new HashSet<int>(protectionTools.Select(x => x.Id));
			foreach(var wearItem in WorkwearItems) {
				if(wearItem.ProtectionTools.MatchedProtectionTools.Any(x => ids.Contains(x.Id)))
					wearItem.UpdateNextIssue(UoW);
			}
		}

		public virtual void UpdateNextIssueAll()
		{
			foreach(var wearItem in WorkwearItems) {
				wearItem.UpdateNextIssue(UoW);
			}
		}

		public virtual void FillWearRecivedInfo(EmployeeIssueRepository issueRepository)
		{
			if (Id == 0) // Не надо проверять выдачи, так как сотрудник еще не сохранен.
				return; 
			foreach(var item in WorkwearItems) {
				item.Amount = 0;
				item.LastIssue = null;
			}

			var receiveds = issueRepository.AllOperationsForEmployee(this).Where(x => x.Issued > 0);
			var protectionGroups = receiveds.Where(x => x.ProtectionTools != null).GroupBy(x => x.ProtectionTools?.Id).ToDictionary(g => g.Key, g => g);

			//Основное заполнение выдачи
			foreach (var item in WorkwearItems)
			{
				if(!protectionGroups.ContainsKey(item.ProtectionTools.Id))
					continue;
				var operations = protectionGroups[item.ProtectionTools.Id];
				//В сортировке OverrideBefore, чтобы в ситуации когда на одну дату есть несколько операция, чтобы выводилась именно ручная.
				var lastOperation = operations.OrderByDescending(x => x.OperationTime.Date).ThenByDescending(x => x.OverrideBefore).First();
				item.Amount = lastOperation.Issued;
				item.LastIssue = lastOperation.OperationTime;
				item.LastIssueOperation = lastOperation;
				protectionGroups.Remove(item.ProtectionTools.Id);
			}
			
			//Дополнительно ищем по аналогам.
			foreach (var item in WorkwearItems)
			{
			 	var matched = item.ProtectionTools.MatchedProtectionTools.FirstOrDefault(x => protectionGroups.ContainsKey(x.Id));
				if(matched == null)
					continue;
				var operations = protectionGroups[matched.Id];
				//В сортировке OverrideBefore, чтобы в ситуации когда на одну дату есть несколько операция, чтобы выводилась именно ручная.
				var lastOperation = operations.OrderByDescending(x => x.OperationTime).ThenByDescending(x => x.OverrideBefore).First();
				item.Amount = lastOperation.Issued;
				item.LastIssue = lastOperation.OperationTime;
				item.LastIssueOperation = lastOperation;
				protectionGroups.Remove(item.ProtectionTools.Id);
			}
		}

		public virtual void FillWearInStockInfo(IUnitOfWork uow, BaseParameters baseParameters, Warehouse warehouse, DateTime onTime, bool onlyUnderreceived = false)
		{
			var actualItems = onlyUnderreceived ? GetUnderreceivedItems(baseParameters) : WorkwearItems;
			FillWearInStockInfo(uow, baseParameters, warehouse, onTime, actualItems);
		}

		public static void FillWearInStockInfo(IUnitOfWork uow, BaseParameters baseParameters, Warehouse warehouse, DateTime onTime, IEnumerable<EmployeeCardItem> items)
		{
			FetchEntitiesInWearItems(uow, items);
			var allNomenclatures = items.SelectMany(x => x.ProtectionTools.MatchedNomenclatures).Distinct().ToList();
			var stockRepo = new StockRepository();
			var stock = stockRepo.StockBalances(uow, warehouse, allNomenclatures, onTime);
			foreach(var item in items) {
				item.InStock = stock.Where(x => item.MatcheStockPosition(x.StockPosition)).ToList();
			}
		}

		public static void FetchEntitiesInWearItems(IUnitOfWork uow, IEnumerable<EmployeeCardItem> cardItems)
		{
			var protectionToolsIds = cardItems.Select(x => x.ProtectionTools.Id).ToArray();

			var query = uow.Session.QueryOver<ProtectionTools>()
				.Where(p => p.Id.IsIn(protectionToolsIds))
				.Fetch(SelectMode.Fetch, p => p.Type)
				.Fetch(SelectMode.Fetch, p => p.Type.Units)
				.Future();

			uow.Session.QueryOver<ProtectionTools>()
				.Where(p => p.Id.IsIn(protectionToolsIds))
				.Fetch(SelectMode.ChildFetch, p => p)
				.Fetch(SelectMode.Fetch, p => p.Analogs)
				.Future();

			uow.Session.QueryOver<ProtectionTools>()
				.Where(p => p.Id.IsIn(protectionToolsIds))
				.Fetch(SelectMode.ChildFetch, p => p)
				.Fetch(SelectMode.Fetch, p => p.Nomenclatures)
				.Future();

			ProtectionTools protectionToolsAnalogAlias = null;

			uow.Session.QueryOver<ProtectionTools>()
				.Where(p => p.Id.IsIn(protectionToolsIds))
				.Fetch(SelectMode.ChildFetch, p => p)
				.JoinAlias(p => p.Analogs, () => protectionToolsAnalogAlias, NHibernate.SqlCommand.JoinType.InnerJoin)
				.Fetch(SelectMode.ChildFetch, analogs => analogs)
				.Fetch(SelectMode.Fetch, () => protectionToolsAnalogAlias.Nomenclatures)
				.Future();

			uow.Session.QueryOver<NormItem>()
				.Where(n => n.Id.IsIn(cardItems.Select(x => x.ActiveNormItem.Id).ToArray()))
				.Future();

			query.ToList();
		}

		#endregion

		#region Функции работы с отпусками

		public virtual void AddVacation(EmployeeVacation vacation)
		{
			vacation.Employee = this;
			ObservableVacations.Add(vacation);
		}

		#endregion

		public virtual void RecalculateDatesOfIssueOperations(IUnitOfWork uow,
			EmployeeIssueRepository employeeIssueRepository, BaseParameters baseParameters,
			IInteractiveQuestion askUser, DateTime begin, DateTime end)
		{
			var operations = employeeIssueRepository.GetOperationsTouchDates(uow, new[] {this}, begin, end,
				q => q.Fetch(SelectMode.Fetch, o => o.ProtectionTools)
			);
			foreach (var typeGroup in operations.GroupBy(o => o.ProtectionTools)) {
				foreach (var operation in typeGroup.OrderBy(o => o.OperationTime.Date).ThenBy(o => o.StartOfUse)) {
					var graph = IssueGraph.MakeIssueGraph(uow, this, typeGroup.Key);
					operation.RecalculateDatesOfIssueOperation(graph, baseParameters, askUser);
					uow.Save(operation);

				}
			}
			UpdateNextIssueAll();
		}
	}
}
