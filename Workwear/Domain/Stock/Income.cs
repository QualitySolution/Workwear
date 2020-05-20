using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Gamma.Utilities;
using Gtk;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities;
using workwear.Domain.Company;
using workwear.Domain.Operations;

namespace workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "приходные документы",
		Nominative = "приходный документ")]
	public class Income : StockDocument, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Свойства

		IncomeOperations operation;

		[Display (Name = "Тип операции")]
		public virtual IncomeOperations Operation {
			get { return operation; }
			set { SetField (ref operation, value, () => Operation); }
		}

		private Warehouse warehouse;

		[Display(Name = "Склад")]
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse {
			get { return warehouse; }
			set { SetField(ref warehouse, value, () => Warehouse); }
		}

		string number;

		[Display (Name = "Вх. номер")]
		public virtual string Number {
			get { return number; }
			set { SetField (ref number, value, () => Number); }
		}

		EmployeeCard employeeCard;

		[Display (Name = "Сотрудник")]
		public virtual EmployeeCard EmployeeCard {
			get { return employeeCard; }
			set { SetField (ref employeeCard, value, () => EmployeeCard); }
		}

		Subdivision subdivision;

		[Display (Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get { return subdivision; }
			set { SetField (ref subdivision, value, () => Subdivision); }
		}

		private IList<IncomeItem> items = new List<IncomeItem>();

		[Display (Name = "Строки документа")]
		public virtual IList<IncomeItem> Items {
			get { return items; }
			set { SetField (ref items, value, () => Items); }
		}

		GenericObservableList<IncomeItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<IncomeItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<IncomeItem> (Items);
				return observableItems;
			}
		}
			
		#endregion

		public virtual string Title{
			get{
				switch (Operation) {
				case IncomeOperations.Enter:
					return String.Format ("Приходная накладная №{0} от {1:d}", Id, Date);
				case IncomeOperations.Return:
					return String.Format ("Возврат от работника №{0} от {1:d}", Id, Date);
				case IncomeOperations.Object:
					return String.Format ("Возврат c объекта №{0} от {1:d}", Id, Date);
				default:
					return null;
				}
			}
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Date < new DateTime(2008, 1, 1))
				yield return new ValidationResult ("Дата должны указана (не ранее 2008-го)", 
					new[] { this.GetPropertyName (o => o.Date)});

			if(Operation == IncomeOperations.Object && Subdivision == null)
				yield return new ValidationResult ("Объект должен быть указан", 
					new[] { this.GetPropertyName (o => o.Date)});

			if(Operation == IncomeOperations.Return && EmployeeCard == null)
				yield return new ValidationResult ("Сотрудник должен быть указан", 
					new[] { this.GetPropertyName (o => o.Date)});

			if(Items.Count == 0)
				yield return new ValidationResult ("Документ должен содержать хотя бы одну строку.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if(Items.Any (i => i.Amount <= 0))
				yield return new ValidationResult ("Документ не должен содержать строк с нулевым количеством.", 
					new[] { this.GetPropertyName (o => o.Items)});

			if (Items.Any(i => i.Certificate != null && i.Certificate.Length > 40))
				yield return new ValidationResult("Длина номера сертификата не может быть больше 40 символов.",
					new[] { this.GetPropertyName(o => o.Items) });

			foreach(var duplicate in Items.GroupBy(x => x.StockPosition).Where(x => x.Count() > 1)) {
				var caseCountText = NumberToTextRus.FormatCase(duplicate.Count(), "{0} раз", "{0} раза", "{0} раз");
				yield return new ValidationResult($"Складская позиция {duplicate.First().Title} указана в документе {caseCountText}.",
					new[] { this.GetPropertyName(o => o.Items) });
			}
		}

		#endregion


		public Income ()
		{
		}

		public virtual void AddItem(SubdivisionIssueOperation issuedOperation, int count)
		{
			if(issuedOperation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			if(Items.Any (p => DomainHelper.EqualDomainObjects (p.IssuedSubdivisionOnOperation, issuedOperation)))
			{
				logger.Warn ("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return;
			}

			var newItem = new IncomeItem(this)
			{
				Amount = count,
				Nomenclature = issuedOperation.Nomenclature,
				IssuedSubdivisionOnOperation= issuedOperation,
				Cost = issuedOperation.CalculatePercentWear(Date),
				WearPercent = issuedOperation.CalculateDepreciationCost(Date)
			};

			ObservableItems.Add (newItem);
		}

		public virtual void AddItem(EmployeeIssueOperation issuedOperation, int count)
		{
			if(issuedOperation.Issued == 0)
				throw new InvalidOperationException("Этот метод можно использовать только с операциями выдачи.");

			if(Items.Any(p => DomainHelper.EqualDomainObjects(p.IssuedEmployeeOnOperation, issuedOperation))) {
				logger.Warn("Номенклатура из этой выдачи уже добавлена. Пропускаем...");
				return;
			}

			var newItem = new IncomeItem(this) {
				Amount = count,
				Nomenclature = issuedOperation.Nomenclature,
				IssuedEmployeeOnOperation = issuedOperation,
				Cost = issuedOperation.CalculateDepreciationCost(Date),
				WearPercent = issuedOperation.CalculatePercentWear(Date),
			};

			ObservableItems.Add(newItem);
		}

		public virtual IncomeItem AddItem(Nomenclature nomenclature)
		{
			if (Operation != IncomeOperations.Enter)
				throw new InvalidOperationException ("Добавление номенклатуры возможно только во входящую накладную. Возвраты должны добавляться с указанием строки выдачи.");
				
			var newItem = new IncomeItem (this) {
				Amount = 1,
				Nomenclature = nomenclature,
				Cost = 0,
			};

			ObservableItems.Add (newItem);
			return newItem;
		}

		public virtual IncomeItem AddItem(Nomenclature nomenclature, int count)
		{
			if(nomenclature == null)
				throw new InvalidOperationException("Номенклатура не найдена");
			var newItem = new IncomeItem(this) {
				Amount = count,
				Nomenclature = nomenclature,
				Cost = 0,
			};

			ObservableItems.Add(newItem);
			return newItem;
		}

		public virtual void RemoveItem(IncomeItem item)
		{
			ObservableItems.Remove (item);
		}

		public virtual void UpdateOperations(IUnitOfWork uow, IInteractiveQuestion askUser)
		{
			Items.ToList().ForEach(x => x.UpdateOperations(uow, askUser));
		}

		#region Загрузка из 1С

		public virtual void StartReadDoc1C()
		{
			XmlElement xRoot = Open1CFile();
			if(xRoot == null) return;
			this.Date = GetDateFrom1CDoc(xRoot);
			GetNomenclature(xRoot);
		}
		public virtual XmlElement Open1CFile()
		{
			XmlDocument xDoc = new XmlDocument();
			XmlElement xRoot = null;
			object[] param = new object[4];
			param[0] = "Cancel";
			param[1] = Gtk.ResponseType.Cancel;
			param[2] = "Open";
			param[3] = Gtk.ResponseType.Accept;

			Gtk.FileChooserDialog fc =
				new Gtk.FileChooserDialog("Open File",
					null,
					Gtk.FileChooserAction.Open,
					param);

			Gtk.FileFilter xmlFilter = new Gtk.FileFilter();
			xmlFilter.Name = "XML";

			if(fc.Run() == (int)Gtk.ResponseType.Accept) {
				if(!fc.Filename.ToLower().EndsWith(".xml")) { fc.Destroy(); return null; }
				xDoc.Load($"{fc.Filename}");
				xRoot = xDoc.DocumentElement;
			}
			fc.Destroy();
			return xRoot;
		}

		public virtual DateTime GetDateFrom1CDoc(XmlElement xRoot)
		{
			DateTime dateTime;
			XmlNodeList childnodes2 = xRoot.SelectNodes("*");
			XmlNodeList childnodes = xRoot.SelectNodes("//Body/Документ.ПеремещениеТоваров/КлючевыеСвойства/Дата");
			foreach(XmlNode ch in childnodes) {
				ch.SelectNodes("//КлючевыеСвойства/Дата");

				foreach(XmlNode c in ch) {
					dateTime = DateTime.Parse(ch.InnerText);
					return dateTime.Date;
				}
			}

			return new DateTime();
		}

		public virtual void GetNomenclature(XmlElement xRoot)
		{
			// проход по всем номенклатурам из файла
			XmlNodeList childnodes = xRoot.SelectNodes("//Body/Документ.ПеремещениеТоваров/Товары/Строка");
			foreach(XmlNode child in childnodes) {
				string ozm = "", count = "";
				var nomenclaturesReference = child.SelectNodes("//ДанныеНоменклатуры/Номенклатура/Ссылка");
				foreach(XmlNode nomReference in nomenclaturesReference)
					ozm = FindNomenclatureOZM(xRoot, nomReference.InnerText);

				var counts= child.SelectNodes("//Количество");
				foreach(XmlNode c in counts) 
					count = c.InnerText;

				if (ozm != "" && count != "")
				AddItem(FindNomenclature(ozm), int.Parse(count));
			}

		}

		public virtual string FindNomenclatureOZM(XmlElement xRoot, string codeNomen)
		{
			XmlNodeList childnodes = xRoot.SelectNodes("//Body/Справочник.Номенклатура");
			bool isFindNomen = false;
			string ozm;

			foreach(XmlNode ch in childnodes) {
				if(ch.SelectSingleNode($"//КлючевыеСвойства[Ссылка = '{codeNomen}']") != null)
					isFindNomen = true;

				if(isFindNomen) {
					foreach(XmlNode c in ch.SelectNodes("//ДополнительныеРеквизиты/Строка/Свойство")) {
						if(c.InnerText.Contains("ОЗМ НЛМК")) {
							ozm = c.InnerText.Replace("ОЗМ НЛМК", "");
							ozm = ozm.Replace("true", "");
							ozm = ozm.Replace("false", "");
							isFindNomen = false;
							return ozm;
						}

					}
				}
			}
			return "";
		}

		public virtual Nomenclature FindNomenclature(string ozm)
		{
			Nomenclature nom = UoW.Session.QueryOver<Nomenclature>().List().FirstOrDefault(x => x.Ozm == ozm);
			return nom;
		}
		#endregion

	}

	public enum IncomeOperations {
		[Display(Name = "Приходная накладная")]
		Enter,
		[Display(Name = "Возврат от работника")]
		Return,
		[Display(Name = "Возврат с объекта")]
		Object

	}

	public class IncomeOperationsType : NHibernate.Type.EnumStringType
	{
		public IncomeOperationsType () : base (typeof(IncomeOperations))
		{
		}
	}

}

