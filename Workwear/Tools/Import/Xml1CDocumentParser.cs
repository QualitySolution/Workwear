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
	public class Xml1CDocumentParser 
	{
		public IUnitOfWork UnitOfWork { get; set; }
		private XDocument Document { get; set; }
		private XElement _1CV8DtUD => (XElement)Document.FirstNode;
		private XNamespace XNamespace => _1CV8DtUD.GetNamespaceOfPrefix("v8");
		private IEnumerable<XElement> CatalogNomenclatures => _1CV8DtUD
			.Descendants(XNamespace + "CatalogObject.Номенклатура")
			.Where(x => (bool)x.Element(XNamespace + "IsFolder") == false);

		private IEnumerable<XElement> CatalogParametersNomenclature => _1CV8DtUD
			.Descendants(XNamespace + "CatalogObject.ХарактеристикиНоменклатуры");


		public void SetData(string filePatch, IUnitOfWork unitOfWork) 
		{
			Document = XDocument.Load(filePatch);
			this.UnitOfWork = unitOfWork;
		}
		public List<Xml1CDocument> ParseDocuments() {
			var documents = _1CV8DtUD
				.Descendants(XNamespace + "DocumentObject.РеализацияТоваровУслуг")
				.Select(eDescendant => new Xml1CDocument(eDescendant, XNamespace))
				.ToList();

			documents
				.AddRange(_1CV8DtUD
					.Descendants(XNamespace + "DocumentObject.ПеремещениеТоваров")
					.Select(eDescendant => new Xml1CDocument(eDescendant, XNamespace)));

			return documents;
		}

		public List<Xml1CDocumentItem> ParseDocumentItems(Xml1CDocument document, bool useAlternativeSize) {
			var elements = document.Root.Elements(XNamespace + "Товары").ToList();
			var documentItems = new List<Xml1CDocumentItem>();
			foreach(var element in elements) {
				var item = new Xml1CDocumentItem(element, XNamespace);
				var nomenclature = ParseNomenclature(item.NomenclatureReference);
				item.Nomenclature = nomenclature.nomenclature;
				item.NomenclatureFromCatalog = nomenclature.nomenclatureName;
				item.NomenclatureArticle = nomenclature.article;
				var sizeAndHeight = ParseSizeAndHeight(
					nomenclature.nomenclature, 
					item.CharacteristicReference, 
					useAlternativeSize);
				item.Size = sizeAndHeight.size;
				item.Height = sizeAndHeight.height;
				item.SizeName = sizeAndHeight.sizeName;
				item.HeightName = sizeAndHeight.heightName;
				documentItems.Add(item);
			}
			return documentItems;
		}

		private (Size size, Size height, string sizeName, string heightName) ParseSizeAndHeight(
			Nomenclature nomenclature, 
			string characteristicReference,
			bool useAlternativeSize) 
		{
			var description = CatalogParametersNomenclature
				.FirstOrDefault(x => x.Element(XNamespace + "Ref")?.Value == characteristicReference)?
				.Element(XNamespace + "Description")?.Value.Trim();
			
			if(nomenclature is null || String.IsNullOrEmpty(description) || description == "б/р" || description == "Б/Р")
				return (null, null, description, description);
			
			Size size = null;
			Size height = null;
			
			var sizeAndHeightNames = ParseSizeAndHeightDescription(description);
			
			if(!String.IsNullOrEmpty(sizeAndHeightNames.Item1) && nomenclature.Type.SizeType != null) {
				var sizeQuery = UnitOfWork.Query<Size>().Where(s => s.SizeType == nomenclature.Type.SizeType);
				if(useAlternativeSize)
					sizeQuery.Where(s => s.AlternativeName == sizeAndHeightNames.Item1 
					                     || (s.AlternativeName == null && s.Name == sizeAndHeightNames.Item1));
				else
					sizeQuery.Where(s => s.Name == sizeAndHeightNames.Item1);
				size = sizeQuery.SingleOrDefault();
			}
			if(!String.IsNullOrEmpty(sizeAndHeightNames.Item2) && nomenclature.Type.HeightType != null){
				var heightQuery = UnitOfWork.Query<Size>().Where(h => h.SizeType == nomenclature.Type.HeightType);
				if(useAlternativeSize)
					heightQuery.Where(h => h.AlternativeName == sizeAndHeightNames.Item2 
					                       || (h.AlternativeName == null && h.Name == sizeAndHeightNames.Item2));
				else 
					heightQuery.And(h => h.Name == sizeAndHeightNames.Item2);
				height = heightQuery.SingleOrDefault();
			}
			return (size, height, sizeAndHeightNames.Item1, sizeAndHeightNames.Item2);
		}

		private (Nomenclature nomenclature, string nomenclatureName, string article) ParseNomenclature(string nomenclatureReference) {
			if(nomenclatureReference is null)
				throw new NullReferenceException();
			var catalogNomenclature = CatalogNomenclatures
				.FirstOrDefault(x => x.Element(XNamespace + "Ref")?.Value == nomenclatureReference);
			var article = catalogNomenclature?.Element(XNamespace + "Артикул")?.Value;
			var nomenclatureName = catalogNomenclature?.Element(XNamespace + "НаименованиеПолное")?.Value;
			if(UInt32.TryParse(article, out var parseResult)) {
				var nomenclature = UnitOfWork.Query<Nomenclature>()
					.Where(x => x.Number == parseResult).List()
					.FirstOrDefault();
				return (nomenclature, nomenclatureName, article);
			}
			return (null, nomenclatureName, $"не удалось получить значение: {article}");
		}
		
		private (string, string) ParseSizeAndHeightDescription(string description) {
			var sizeAndHeightRegex = new Regex(@"^[0-9]{1,3}-[0-9]{1,3}/[0-9]{1,3}-[0-9]{1,3}$", 
				RegexOptions.Compiled);
			var sizeAndHeightImportRegex = new Regex(@"^\S+\({1}[0-9]{1,3}-[0-9]{1,3}/[0-9]{1,3}-[0-9]{1,3}\){1}$", 
				RegexOptions.Compiled);
			var sizeWithAnnotationRegex = new Regex(@"\w*размер\w*", RegexOptions.Compiled);
			var gloveSizeRegex = new Regex(@"^[0-9]{1,3}(?:[.,][0-9])?-[0-9]{1,3}(?:[.,][0-9])?/\S+$", RegexOptions.Compiled);
			var multipleXRegex = new Regex(@"^\d{1}X{1}\S$", RegexOptions.Compiled);

			if(sizeAndHeightRegex.IsMatch(description)) {
				var sizeAndHeight = description.Split('/');
				return (sizeAndHeight[0], sizeAndHeight[1]);
			}
			
			if(sizeAndHeightImportRegex.IsMatch(description)) {
				var cutDescription = description.Remove(0, description.IndexOf('(') + 1).TrimEnd(')');
				var sizeAndHeight = cutDescription.Split('/');
				return (sizeAndHeight[0], sizeAndHeight[1]);
			}
			
			if(gloveSizeRegex.IsMatch(description)) {
				var startRemove = description.IndexOf('/');
				var cutDescription = description.Remove(startRemove, description.Length - startRemove);
				return (cutDescription, null);
			}

			if(sizeWithAnnotationRegex.IsMatch(description)) {
				var replaceRegex = new Regex(@"/D", RegexOptions.Compiled);
				var sizeName = replaceRegex.Replace(description, String.Empty).Trim();
				return (sizeName, null);
			}

			if(multipleXRegex.IsMatch(description)) {
				var multiplier = Int32.Parse($"{description.First()}");
				string sizeName;
				if(multiplier <= 3) {
					sizeName = new string('X', multiplier) + description.Remove(0, 2);
				}
				else { //Если больше 3X то в базе храним с числом 
					sizeName = description;
				}
				return (sizeName, null);
			}
			
			return (description, description);
		}
	}
}
