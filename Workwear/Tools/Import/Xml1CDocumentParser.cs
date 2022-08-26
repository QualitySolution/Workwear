using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;
using workwear.Domain.Stock;

namespace workwear.Tools.Import 
{
	public class Xml1CDocumentParser 
	{
		public IUnitOfWork UnitOfWork { get; set; }
		private XDocument Document { get; set; }
		private XElement _1CV8DtUD => (XElement)Document.FirstNode;
		
		private XNamespace xNamespace => _1CV8DtUD.GetNamespaceOfPrefix("v8");
		
		
		public void SetDocument(string filePatch) 
		{
			Document = XDocument.Load(filePatch);
		}
		public List<Xml1CDocument> ParseDocuments() {
			var documents = _1CV8DtUD
				.Descendants(xNamespace + "DocumentObject.РеализацияТоваровУслуг")
				.Select(eDescendant => new Xml1CDocument(eDescendant, xNamespace))
				.ToList();

			documents
				.AddRange(_1CV8DtUD
					.Descendants(xNamespace + "DocumentObject.ПеремещениеТоваров")
					.Select(eDescendant => new Xml1CDocument(eDescendant, xNamespace)));

			return documents;
		}

		public List<Xml1CDocumentItem> ParseDocumentItems(Xml1CDocument document) {
			var elements = document.Root.Elements(xNamespace + "Товары").ToList();
			var documentItems = new List<Xml1CDocumentItem>();
			foreach(var element in elements) {
				var item = new Xml1CDocumentItem(element, xNamespace);
				var nomenclature = ParseNomenclature(item.NomenclatureReference);
				item.Nomenclature = nomenclature.nomenclature;
				item.NomenclatureFromCatalog = nomenclature.nomenclatureName;
				var sizeAndHeight = ParseSizeAndHeight(item.CharacteristicReference);
				item.Size = sizeAndHeight.size;
				item.Height = sizeAndHeight.height;
				item.CharacteristicFromCatalog = sizeAndHeight.characteristic;
				documentItems.Add(item);
			}
			return documentItems;
		}

		private (Size size, Size height, string characteristic) ParseSizeAndHeight(string itemNomenclatureCharacteristic) {
			return (null, null, "sgfsr");
		}

		private (Nomenclature nomenclature, string nomenclatureName) ParseNomenclature(string nomenclatureReference) {
			return (null, "rsgtf");
		}
	}
}
