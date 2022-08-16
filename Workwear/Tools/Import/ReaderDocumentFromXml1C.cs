using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;
using workwear.Domain.Stock;

namespace workwear.Tools.Import 
{
	public class ReaderDocumentFromXml1C 
	{
		private readonly IUnitOfWork unitOfWork;
		private List<XElement> documents1C;
		private readonly XElement _1CV8DtUD;
		private List<XElement> nomenclatures;
		private readonly XNamespace nsV8;
		public DateTime? DocumentDate{ get; private set; }
		public IList<Xml1CDocumentItem> DocumentItems { get; private set; }
		public IList<NotFoundNomenclature> NotFoundNomenclatures { get; set; }
		public IList<string> NotFoundNomenclatureNumbers { get; set; }

		public ReaderDocumentFromXml1C(string fileName, IUnitOfWork unitOfWork) {
			this.unitOfWork = unitOfWork;
			var document = XDocument.Load(fileName);
			_1CV8DtUD = (XElement)document.FirstNode;
			nsV8 = _1CV8DtUD.GetNamespaceOfPrefix("v8");
			ParseDocuments();
			ReadDate();
			ReadNomenclature();
			ReadItems();
		}

		private void ReadNomenclature() {
			nomenclatures = _1CV8DtUD
				.Descendants(nsV8 + "CatalogObject.Номенклатура")
				.Where(x => (bool)x.Element(nsV8 + "IsFolder") == false)
				.ToList();
		}

		private void ParseDocuments() {
			documents1C = new List<XElement>();
			documents1C.AddRange(_1CV8DtUD.Descendants(nsV8 + "DocumentObject.РеализацияТоваровУслуг").ToList());
			documents1C.AddRange(_1CV8DtUD.Descendants(nsV8 + "DocumentObject.ПеремещениеТоваров").ToList());
		}


		private void ReadItems() {
			DocumentItems = new List<Xml1CDocumentItem>();
			foreach(var document1C in documents1C) {
				var items = document1C.Elements(nsV8 + "Товары").ToList();
				foreach(var item in items) {
					var nomenclature = ParseNomenclature(item.Element(nsV8 + "Номенклатура")?.Value);
					if(nomenclature != null) {
						var sizeAndHeight = ParseSizeAndHeight(item.Element(nsV8 + "Характеристика")?.Value, nomenclature);
						var documentItem = new Xml1CDocumentItem {
							Namenclature = nomenclature,
							Size = sizeAndHeight.Item1,
							Height = sizeAndHeight.Item2,
							Amount = Int32.TryParse(item.Element(nsV8 + "Количество")?.Value, out var amountParseResult) ? 
								amountParseResult : 0,
							Cost = Decimal.TryParse(item.Element(nsV8 + "Цена")?.Value, out var costParseResult) ? costParseResult : 0
						};
						DocumentItems.Add(documentItem);
					}
				}
			}
		}

		private (Size, Size) ParseSizeAndHeight(string catalogReference, Nomenclature nomenclature) {
			return (null, null);
		}

		private void ReadDate() {
			var date = documents1C.First().Element( nsV8 + "Date")?.Value;
			if(date != null && DateTime.TryParse(date, out var resultParse))
				DocumentDate =resultParse;
		}

		private Nomenclature ParseNomenclature(string catalogReference) {
			NotFoundNomenclatures = new List<NotFoundNomenclature>();
			NotFoundNomenclatureNumbers = new List<string>();
			if(catalogReference is null)
				return null;
			var catalogNomenclature = nomenclatures.FirstOrDefault(x => x.Element(nsV8 + "Ref")?.Value == catalogReference);
			var article = catalogNomenclature?.Element(nsV8 + "Артикул")?.Value;
			var nomenclatureName = catalogNomenclature?.Element(nsV8 + "НаименованиеПолное")?.Value;
			if(article is null)
				NotFoundNomenclatureNumbers.Add(nomenclatureName);
			if(UInt32.TryParse(article, out var number)) {
				return unitOfWork.Query<Nomenclature>().Where(x => x.Number == number).List().FirstOrDefault();
			}
			else if(article is null) {
				NotFoundNomenclatureNumbers.Add(nomenclatureName);
				return null;
			}
			else {
				NotFoundNomenclatures.Add(new NotFoundNomenclature {
					Name = nomenclatureName,
					Article = number
				});
				return null;
			}
		}
	}

	public class Xml1CDocumentItem 
	{
		public Nomenclature Namenclature { get; set; }
		public Size Size { get; set; }
		public Size Height { get; set; }
		public int Amount { get; set; }
		public decimal Cost { get; set; }
	}


	public class NotFoundNomenclature {
		public string Name { get; set; }
		public uint Article { get; set; }
	}
}
