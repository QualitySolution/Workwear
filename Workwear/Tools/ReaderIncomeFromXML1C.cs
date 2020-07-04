using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using workwear.Domain.Stock;
using QS.DomainModel.UoW;
using QS.DomainModel.Entity;

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
			nsMgr.AddNamespace("df", "http://v8.1c.ru/edi/edi_stnd/EnterpriseData/1.3");

			List<string> listNomenReference = new List<string>();
			List<string> listNomenCount = new List<string>();
			List<string> listNomenName = new List<string>();

			listNomenReference = getXmlNodeChild(xRoot, nsMgr, "//df:Body/df:Документ.ПеремещениеТоваров/df:Товары/df:Строка/df:ДанныеНоменклатуры/df:Номенклатура/df:Ссылка");
			listNomenCount = getXmlNodeChild(xRoot, nsMgr, "//df:Body/df:Документ.ПеремещениеТоваров/df:Товары/df:Строка/df:Количество");
			listNomenName = getXmlNodeChild(xRoot, nsMgr, "//df:Body/df:Документ.ПеремещениеТоваров/df:Товары/df:Строка/df:ДанныеНоменклатуры/df:Характеристика/df:НаименованиеПолное");

			XDocument doc = XDocument.Load(file);
			XNamespace ns = "http://v8.1c.ru/edi/edi_stnd/EnterpriseData/1.3";
			int i = 0;
			foreach(var nomenReference in listNomenReference) {
				var ozm = (from feed in doc.Descendants(ns + "Справочник.Номенклатура")
						   from et in feed.Elements(ns + "КлючевыеСвойства")
						   where (string)et.Element(ns + "Ссылка") == nomenReference
						   select feed.Element(ns + "ДополнительныеРеквизиты")
						   .Element(ns + "Строка").Element(ns + "ЗначениеСвойства").Element(ns + "Число")).ToList().First().Value;

				var sizeGrowth = getSizeAndGrowth(listNomenName[i]);
				Nomenclature nom = FindNomenclature(ozm);

				if(nom == null) {
					if(listDontFindNomenclature.FirstOrDefault(x => x.Ozm.ToString() == ozm) == null)
						listDontFindNomenclature.Add(new LineIncome(listNomenName[i], uint.Parse(ozm), listNomenCount[i], sizeGrowth[0], sizeGrowth[1]));
				}
				else {
					ListLineIncomes.Add(new LineIncome(nom, listNomenCount[i], sizeGrowth[0], sizeGrowth[1]));
				}

				i++;
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
			bool isSize = true;
			foreach(var vorld in onlySize) {
				if(vorld == '/' && size1.Length > 0) {
					isSize = false;
					size2 = int.Parse(number) > 70 ? (int.Parse(number) / 2).ToString() : number;
					number = "";
				}
				else if(vorld == '/') {
					isSize = false;
					size1 = int.Parse(number) > 70 ? (int.Parse(number) / 2).ToString() : number;
					number = "";
				}
				else if(vorld == '-' && isSize) {
					size1 = int.Parse(number) > 70 ? (int.Parse(number) / 2).ToString() : number;
					number = "";
				}
				else if(vorld == '-' && !isSize) {
					growth1 = number;
					number = "";
				}
				else if(char.IsDigit(vorld))
					number += vorld;
				else if(!char.IsDigit(vorld))
					break;
				if(growth1.Length > 0 && number.Length > 0)
					growth2 = number;
				if(size1.Length < 1 && number.Length > 0)
					size1 = number;
			}

			mass[0] = size2.Length > 0 ? size1 + "-'" + size2 : size1;
			mass[1] = growth2.Length > 0 ? growth1 + "-" + growth2 : growth1;

			return mass;
		}

		public virtual Nomenclature FindNomenclature(string ozm)
		{
			Nomenclature nom = UoW.Session.QueryOver<Nomenclature>().List().FirstOrDefault(x => x.Ozm.ToString() == ozm);
			return nom;
		}

	}

	public class LineIncome : PropertyChangedBase
	{
		public Nomenclature Nomenclature;
		public string Name;
		public uint? Ozm;
		public string Count;
		public string Size;
		public string Growth;

		public LineIncome(Nomenclature nom, string count, string size, string growth)
		{
			this.Nomenclature = nom;
			this.Count = count;
			this.Size = size;
			this.Growth = growth;
		}

		public LineIncome(string name, uint ozm, string count, string size, string growth)
		{
			this.Name = name;
			this.Count = count;
			this.Size = size;
			this.Growth = growth;
			this.Ozm = ozm;
		}
	}
}
