using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using QS.DomainModel.UoW;

namespace workwear.Tools.Import 
{
	public class Xml1CDocumentParser 
	{
		public IUnitOfWork UnitOfWork { get; set; }
		public List<Xml1CDocument> ParseDocuments(string filePatch) 
		{
			var document = XDocument.Load(filePatch);
			var _1CV8DtUD = (XElement)document.FirstNode;
			var nsV8 = _1CV8DtUD.GetNamespaceOfPrefix("v8");
			var documents = _1CV8DtUD
				.Descendants(nsV8 + "DocumentObject.РеализацияТоваровУслуг")
				.Select(eDescendant => new Xml1CDocument(eDescendant, nsV8))
				.ToList();

			documents
				.AddRange(_1CV8DtUD
					.Descendants(nsV8 + "DocumentObject.ПеремещениеТоваров")
					.Select(eDescendant => new Xml1CDocument(eDescendant, nsV8)));

			return documents;
		}

		public List<Xml1CDocumentItem> ParseDocumentItems(Xml1CDocument document) 
		{
			return new List<Xml1CDocumentItem>();
		}
	}
}
