using System;
using System.Xml.Linq;
using Workwear.Domain.Sizes;
using workwear.Domain.Stock;

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
		public int Amount => Int32.TryParse(AmountFromCatalog, out var parseResult) ? parseResult : 0;
		public decimal Cost => Decimal.TryParse(CostFromCatalog, out var costParseResult) ? costParseResult : 0;
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
