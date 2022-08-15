using System;
using System.Collections.Generic;
using System.Xml;
using Workwear.Domain.Sizes;
using workwear.Domain.Stock;

namespace workwear.Tools.Import 
{
	public class ReaderDocumentFromXml1C 
	{
		private readonly XmlDocument document;
		private XmlNode rootElement;
		public DateTime? DocumentDate{ get; private set; }
		public IList<Xml1CDocumentItem> DocumentItems { get; }

		public ReaderDocumentFromXml1C(string fileName) {
			document = new XmlDocument();
			document.Load($"{fileName}");
			ParseDocumentFields();
			ReadDate();
			ReadItems();
		}

		private void ParseDocumentFields() {
			XmlNode root;
			root = document.DocumentElement?.SelectSingleNode("//*[local-name()='DocumentObject.РеализацияТоваровУслуг']");
			if(root != null) {
				rootElement = root;
				return;
			}
			root = document.DocumentElement?.SelectSingleNode("//*[local-name()='DocumentObject.ПеремещениеТоваров']");
			if(root != null) {
				rootElement = root;
			}
		}


		private void ReadItems() {
			
		}

		private void ReadDate() {
			var data = rootElement.SelectSingleNode("//*[local-name()='Date']")?.InnerText;
			if(data != null)
				DocumentDate = DateTime.Parse(data);
		}
	}

	public class Xml1CDocumentItem 
	{
		public Nomenclature Namenclature { get; set; }
		public Size Size { get; set; }
		public Size Height { get; set; }
		public int Ammount { get; set; }
	}
}
