using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
		private List<XElement> parametersNomenclature;
		private readonly XNamespace nsV8;
		private readonly bool useAlternativeSize;
		public DateTime? DocumentDate{ get; private set; }
		public IList<Xml1CDocumentItem> DocumentItems { get; private set; }
		public IList<NotFoundNomenclature> NotFoundNomenclatures { get; set; }
		public IList<string> NotFoundNomenclatureNumbers { get; set; }

		public ReaderDocumentFromXml1C(string fileName, IUnitOfWork unitOfWork,bool useAlternativeSize) {
			this.unitOfWork = unitOfWork;
			this.useAlternativeSize = useAlternativeSize;
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
			parametersNomenclature = _1CV8DtUD.Descendants(nsV8 + "CatalogObject.ХарактеристикиНоменклатуры").ToList();
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
						AddDocumentItems(documentItem);
					}
				}
			}
		}

		private void AddDocumentItems(Xml1CDocumentItem documentItem) {
			var item = DocumentItems.FirstOrDefault(x =>
				x.Namenclature == documentItem.Namenclature
				&& x.Cost == documentItem.Cost
				&& x.Height == documentItem.Height
				&& x.Size == documentItem.Size);
			if(item is null)
				DocumentItems.Add(documentItem);
			else
				item.Amount += documentItem.Amount;
		}

		private (Size, Size) ParseSizeAndHeight(string catalogReference, Nomenclature nomenclature) {
			var description = parametersNomenclature
				.FirstOrDefault(x => x.Element(nsV8 + "Ref")?.Value == catalogReference)?
				.Element(nsV8 + "Description")?.Value.Trim();
			if(String.IsNullOrEmpty(description) || description == "б/р")
				return (null, null);
			
			Size size = null;
			Size height = null;
			var sizeAndHeightNames = ParseSizeAndHeightDescription(description);
			if(nomenclature.Type.SizeType != null) {
				var sizeQuery = unitOfWork.Query<Size>()
					.Where(s => s.SizeType == nomenclature.Type.SizeType);
				if(useAlternativeSize)
					sizeQuery.Where(s => 
						s.AlternativeName == sizeAndHeightNames.Item1 
						|| (s.AlternativeName == null && s.Name == sizeAndHeightNames.Item1));
				else
					sizeQuery.Where(s => s.Name == sizeAndHeightNames.Item1);

				size = sizeQuery.SingleOrDefault();
			}
			if(nomenclature.Type.HeightType != null){
				var heightQuery = unitOfWork.Query<Size>()
					.Where(h => h.SizeType == nomenclature.Type.HeightType);
				if(useAlternativeSize)
					heightQuery.Where(h => h.AlternativeName == sizeAndHeightNames.Item2 
					                       || (h.AlternativeName == null && h.Name == sizeAndHeightNames.Item2));
				else 
					heightQuery.And(h => h.Name == sizeAndHeightNames.Item2);
				
				height = heightQuery.SingleOrDefault();
			}
			return (size, height);
		}

		private (string, string) ParseSizeAndHeightDescription(string description) {
			var sizeAndHeightRegex = new Regex(@"^[0-9]{1,3}-[0-9]{1,3}/[0-9]{1,3}-[0-9]{1,3}$", RegexOptions.Compiled);
			var sizeWithAnnotation = new Regex(@"\w*размер\w*", RegexOptions.Compiled);

			if(sizeAndHeightRegex.IsMatch(description)) {
				var sizeAndHeight = description.Split('/');
				return (sizeAndHeight[0], sizeAndHeight[1]);
			}

			if(sizeWithAnnotation.IsMatch(description)) {
				var replaceRegex = new Regex(@"/D", RegexOptions.Compiled);
				description = replaceRegex.Replace(description, "").Trim();
				return (description, description);
			}
			return (description, description);
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
				var nomenclature = unitOfWork.Query<Nomenclature>().Where(x => x.Number == number).List().FirstOrDefault();
				if(nomenclature != null)
					return nomenclature;
				else {
					NotFoundNomenclatures.Add(new NotFoundNomenclature {
						Name = nomenclatureName,
						Article = number
					});
					return null;
				}
			}
			return null;
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
