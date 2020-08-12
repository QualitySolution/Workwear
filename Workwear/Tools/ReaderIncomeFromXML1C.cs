using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using workwear.Domain.Stock;

namespace workwear.Tools
{
	public class ReaderIncomeFromXML1C
	{
		string nameFile;
		public string NameFile {
			get { return nameFile; }
			set { nameFile = value; }
		}

		DateTime date;
		public DateTime Date {
			get { return date; }
			set { date = value; }
		}

		XmlElement xRoot;
		public IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();

		public IList<LineIncome> ListLineIncomes = new List<LineIncome>();
		public IList<LineIncome> listDontFindNomenclature = new List<LineIncome>();
		public IList<string> listDontFindOZMInDoc = new List<string>();

		public ReaderIncomeFromXML1C(string nameFile)
		{
			this.NameFile = nameFile;
		}

		public void StartReadDoc1C()
		{
			XmlDocument xDoc = new XmlDocument(); ;
			xDoc.Load($"{nameFile}");
			xRoot = xDoc.DocumentElement;
			Date = GetDateFrom1CDoc(xRoot);
			GetNomenclature(xRoot, NameFile);
		}

		public virtual DateTime GetDateFrom1CDoc(XmlElement xRoot)
		{
			DateTime dateTime = new DateTime();
			XmlNodeList childnodes =
			xRoot.SelectNodes("//*[name()='Body']/*[name()='Документ.ПеремещениеТоваров']/*[name()='КлючевыеСвойства']//*[name()='Дата']");
			foreach(XmlNode ch in childnodes)
				dateTime = DateTime.Parse(ch.InnerText);
			return dateTime.Date;
		}

		public virtual void GetNomenclature(XmlElement xRoot, string file)
		{
			XmlNamespaceManager nsMgr = new XmlNamespaceManager(new NameTable());
			nsMgr.AddNamespace("df", "http://v8.1c.ru/edi/edi_stnd/EnterpriseData/1.8");

			List<string> listNomenReference = new List<string>();
			List<string> listNomenCount = new List<string>();
			List<string> listNomenName = new List<string>();

			listNomenReference = getXmlNodeChild(xRoot, nsMgr, "//df:Body/df:Документ.ПеремещениеТоваров/df:Товары/df:Строка/df:ДанныеНоменклатуры/df:Номенклатура/df:Ссылка");
			listNomenCount = getXmlNodeChild(xRoot, nsMgr, "//df:Body/df:Документ.ПеремещениеТоваров/df:Товары/df:Строка/df:Количество");
			listNomenName = getXmlNodeChild(xRoot, nsMgr, "//df:Body/df:Документ.ПеремещениеТоваров/df:Товары/df:Строка/df:ДанныеНоменклатуры/df:Номенклатура/df:НаименованиеПолное");
			var listNomenNameFull = getXmlNodeChild(xRoot, nsMgr, "//df:Body/df:Документ.ПеремещениеТоваров/df:Товары/df:Строка/df:ДанныеНоменклатуры/df:Характеристика/df:НаименованиеПолное");

			if(listNomenReference.Count != listNomenCount.Count)
				throw new InvalidOperationException("Количество элементов в списках listNomenReference и listNomenCount не равно.");

			if(listNomenReference.Count != listNomenName.Count)
				throw new InvalidOperationException("Количество элементов в списках listNomenReference и listNomenName не равно.");

			if(listNomenReference.Count != listNomenNameFull.Count)
				throw new InvalidOperationException("Количество элементов в списках listNomenReference и listNomenNameFull не равно.");

			XDocument doc = XDocument.Load(file);
			XNamespace ns = "http://v8.1c.ru/edi/edi_stnd/EnterpriseData/1.8";
			int i = -1;
			foreach(var nomenReference in listNomenReference) {
				i++;
				var ozm = "";
				try {
					ozm = (from feed in doc.Descendants(ns + "Справочник.Номенклатура")
							   from et in feed.Elements(ns + "КлючевыеСвойства")
							   where (string)et.Element(ns + "Ссылка") == nomenReference
							   select feed.Element(ns + "ДополнительныеРеквизиты")
							   .Element(ns + "Строка").Element(ns + "ЗначениеСвойства").Element(ns + "Число")).ToList().First().Value;
				}
				catch {
					if (!listDontFindOZMInDoc.Contains(listNomenName[i]))
						listDontFindOZMInDoc.Add(listNomenName[i]);
				  }

				Nomenclature nom = null;
				if(ozm != "") {
					if(int.TryParse(ozm, out int ozmNum))
						nom = FindNomenclature(ozmNum);
				}
				else nom = FindNomenclature(listNomenName[i]);

				var sizeGrowth = getSizeAndGrowth(listNomenNameFull[i]);

				if(nom == null) {
					if((ozm != "" && !listDontFindNomenclature.Any(x => x.Ozm.ToString() == ozm)) || (ozm == "" && !listDontFindNomenclature.Any(x => x.Name == listNomenName[i])))
						listDontFindNomenclature.Add(new LineIncome(listNomenName[i], ozm, int.Parse(listNomenCount[i]), sizeGrowth[0], sizeGrowth[1]));
				}
				else {
					LineIncome find = ListLineIncomes.FirstOrDefault(x => x.Nomenclature == nom 
						&& x.Size == sizeGrowth[0]
						&& x.Growth == sizeGrowth[1]
					 );
					if (find != null) 
						find.Count += int.Parse(listNomenCount[i]);
					else 
						ListLineIncomes.Add(new LineIncome(nom, int.Parse(listNomenCount[i]), sizeGrowth[0], sizeGrowth[1]));
				}
			}

		}

		public virtual List<string> getXmlNodeChild(XmlElement xRoot, XmlNamespaceManager nsMgr, string strFind)
		{
			List<string> list = new List<string>();
			XmlNodeList childnodesNomenCount = xRoot.SelectNodes(strFind, nsMgr);
			foreach(XmlNode child in childnodesNomenCount)
				list.Add(child.InnerText);
			return list;
		}

		public virtual string[] getSizeAndGrowth(string element)
		{
			string[] mass = new string[2];

			string[] parts = element.Split(' ');
			string onlySize = parts[0];
			string size1 = "", size2 = "", growth1 = "", growth2 = "", number = "";
			bool isHyphen1 = false, isHyphen2 = false, isSeparator = false;

			List<string> sizeWear = new List<string>() {"M", "L", "XL", "XXL", "XXXL", "4XL", "5XL" };

			if(onlySize.Where(x => x == 'L').Count() == 1) {
				onlySize = onlySize.Replace('2', 'X');
				onlySize = onlySize.Replace("3", "XX");
			}

			if(sizeWear.Contains(onlySize))
				size1 = onlySize;

			foreach(var character in onlySize) {
				if(char.IsDigit(character))
					number += character;
				else if(character == '-' && !isSeparator) {
					isHyphen1 = true;
					number = "";
					continue;
				}
				else if(character == '-' && isSeparator) {
					isHyphen2 = true;
					number = "";
					continue;
				}
				else if(character == '/') {
					isSeparator = true;
					number = "";
					continue;
				}
				else { number = ""; break; }

				if(number.Count() > 0 && !isHyphen1 && !isHyphen2 && !isSeparator)
					size1 = int.Parse(number) > 70 ? (int.Parse(number) / 2).ToString() : number;
				else if(number.Count() > 0 && isHyphen1 && !isHyphen2 && !isSeparator)
					size2 = int.Parse(number) > 70 ? (int.Parse(number) / 2).ToString() : number;
				else if(number.Count() > 0 && !isHyphen1 && !isHyphen2 && isSeparator)
					growth1 = number;
				else if(number.Count() > 0 && isHyphen1 && !isHyphen2 && isSeparator)
					growth1 = number;
				else if(number.Count() > 0 && isHyphen1 && isHyphen2 && isSeparator)
					growth2 = number;
				else if(number.Count() > 0 && !isHyphen1 && isHyphen2 && isSeparator)
					growth2 = number;
			}

			mass[0] = size2.Length > 0 ? size1 + "-" + size2 : size1;
			mass[1] = growth2.Length > 0 ? growth1 + "-" + growth2 : growth1;

			return mass;
		}

		public virtual Nomenclature FindNomenclature(int ozm)
		{
			Nomenclature nom = UoW.Session.QueryOver<Nomenclature>().Where(x => x.Ozm == ozm).Take(1).SingleOrDefault();
			return nom;
		}

		public virtual Nomenclature FindNomenclature(string name)
		{
			Nomenclature nom = UoW.Session.QueryOver<Nomenclature>().Where(x => x.Name == name).Take(1).SingleOrDefault();
			return nom;
		}

	}

	public class LineIncome : PropertyChangedBase
	{
		public Nomenclature Nomenclature;
		public string Name;
		public uint? Ozm;
		public int Count;
		public string Size;
		public string Growth;

		public LineIncome(Nomenclature nom, int count, string size, string growth)
		{
			this.Nomenclature = nom;
			this.Name = nom.Name;
			this.Count = count;
			this.Size = size;
			this.Growth = growth;
		}

		public LineIncome(string name, string ozm, int count, string size, string growth)
		{
			this.Name = name;
			this.Count = count;
			this.Size = size;
			this.Growth = growth;
			if(ozm == "")
				this.Ozm = null;
			else this.Ozm = uint.Parse(ozm); 
		}
	}
}
