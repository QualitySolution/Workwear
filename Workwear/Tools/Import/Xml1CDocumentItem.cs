using System;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;

namespace workwear.Tools.Import 
{
	public class Xml1CDocumentItem 
	{
		public Xml1CDocumentItem(XElement element, XNamespace xNamespace) {
			root = element;
			this.xNamespace = xNamespace;
		}

		#region private field

		private readonly XElement root;
		private readonly XNamespace xNamespace;

		#endregion

		#region public property

		public Nomenclature Nomenclature { get; set; }
		public Size Size { get; set; }
		public Size Height { get; set; }
		private int? amount;
		public int Amount {
			get {
				if(amount is null)
					amount = Int32.TryParse(AmountFromCatalog, out var parseResult) ? parseResult : 0;
				return amount.Value;
			}
			set => amount = value;
		}

		private decimal? cost;
		public decimal Cost {
			get {
				if(cost is null) 
					cost = XmlConvert.ToDecimal(CostFromCatalog);
				return cost.Value;
			}
			set => cost = value;
		}

		public string NomenclatureFromCatalog { get; set; }
		public string NomenclatureArticle { get; set; }
		public string SizeName { get; set; }
		public string HeightName { get; set; }
		public string AmountFromCatalog => root.Element(xNamespace + "Количество")?.Value;
		public string CostFromCatalog => root.Element(xNamespace + "Цена")?.Value;

		#endregion

		#region CatalogReferences

		public string NomenclatureReference => root.Element(xNamespace + "Номенклатура")?.Value;
		public string CharacteristicReference => root.Element(xNamespace + "Характеристика")?.Value;

		#endregion
	}
}
