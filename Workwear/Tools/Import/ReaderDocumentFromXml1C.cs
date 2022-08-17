﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;
using workwear.Domain.Stock;

namespace workwear.Tools.Import 
{
	public class ReaderDocumentFromXml1C 
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly List<XElement> documents1C = new List<XElement>();
		private readonly XElement _1CV8DtUD;
		private List<XElement> nomenclatures;
		private List<XElement> parametersNomenclature;
		private readonly XNamespace nsV8;
		private readonly bool useAlternativeSize;
		private readonly IProgressBarDisplayable progressBar;
		public DateTime? DocumentDate{ get; private set; }
		public IList<Xml1CDocumentItem> DocumentItems { get; } = new List<Xml1CDocumentItem>();
		public IList<NotFoundNomenclature> NotFoundNomenclatures { get; } = new List<NotFoundNomenclature>();
		public IList<string> NotFoundNomenclatureNumbers { get; } = new List<string>();
		public IList<string> UnreadableArticle { get; } = new List<string>();

		public ReaderDocumentFromXml1C(
			string fileName, 
			IUnitOfWork unitOfWork, 
			IProgressBarDisplayable progressBar, 
			bool useAlternativeSize) 
		{
			this.unitOfWork = unitOfWork;
			this.progressBar = progressBar;
			this.useAlternativeSize = useAlternativeSize;
			var document = XDocument.Load(fileName);
			_1CV8DtUD = (XElement)document.FirstNode;
			nsV8 = _1CV8DtUD.GetNamespaceOfPrefix("v8");
			ParseDocuments();
			ReadDate();
			ReadNomenclatureAndParameters();
			ReadItems();
		}

		private void ReadNomenclatureAndParameters() {
			nomenclatures = _1CV8DtUD
				.Descendants(nsV8 + "CatalogObject.Номенклатура")
				.Where(x => (bool)x.Element(nsV8 + "IsFolder") == false)
				.ToList();
			parametersNomenclature = _1CV8DtUD
				.Descendants(nsV8 + "CatalogObject.ХарактеристикиНоменклатуры")
				.ToList();
		}

		private void ParseDocuments() {
			documents1C.AddRange(_1CV8DtUD.Descendants(nsV8 + "DocumentObject.РеализацияТоваровУслуг").ToList());
			documents1C.AddRange(_1CV8DtUD.Descendants(nsV8 + "DocumentObject.ПеремещениеТоваров").ToList());
		}


		private void ReadItems() {
			var items = documents1C.SelectMany(d => d.Elements(nsV8 + "Товары")).ToList();
			progressBar.Start(items.Count, 0, "загрузка документа");
			foreach(var item in items) {
				progressBar.Add();
				var nomenclature = ParseNomenclature(item.Element(nsV8 + "Номенклатура")?.Value);
				if(nomenclature != null) {
					var sizeAndHeight = ParseSizeAndHeight(item.Element(nsV8 + "Характеристика")?.Value, nomenclature);
					var documentItem = new Xml1CDocumentItem {
						Nomenclature = nomenclature,
						Size = sizeAndHeight.Item1,
						Height = sizeAndHeight.Item2,
						Amount =
							Int32.TryParse(item.Element(nsV8 + "Количество")?.Value, out var amountParseResult) ? amountParseResult : 0,
						Cost = Decimal.TryParse(item.Element(nsV8 + "Цена")?.Value, out var costParseResult) ? costParseResult : 0
					};
					AddDocumentItems(documentItem);
				}
			}
			progressBar.Close();
		}

		private void AddDocumentItems(Xml1CDocumentItem documentItem) {
			var item = DocumentItems.FirstOrDefault(x =>
				x.Nomenclature == documentItem.Nomenclature
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
			
			if(!String.IsNullOrEmpty(sizeAndHeightNames.Item1) && nomenclature.Type.SizeType != null) {
				var sizeQuery = unitOfWork.Query<Size>().Where(s => s.SizeType == nomenclature.Type.SizeType);
				if(useAlternativeSize)
					sizeQuery.Where(s => s.AlternativeName == sizeAndHeightNames.Item1 
					                     || (s.AlternativeName == null && s.Name == sizeAndHeightNames.Item1));
				else
					sizeQuery.Where(s => s.Name == sizeAndHeightNames.Item1);
				size = sizeQuery.SingleOrDefault();
			}
			if(!String.IsNullOrEmpty(sizeAndHeightNames.Item2) && nomenclature.Type.HeightType != null){
				var heightQuery = unitOfWork.Query<Size>().Where(h => h.SizeType == nomenclature.Type.HeightType);
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
			var sizeAndHeightRegex = new Regex(@"^[0-9]{1,3}-[0-9]{1,3}/[0-9]{1,3}-[0-9]{1,3}$", 
				RegexOptions.Compiled);
			var sizeWithAnnotationRegex = new Regex(@"\w*размер\w*", RegexOptions.Compiled);

			if(sizeAndHeightRegex.IsMatch(description)) {
				var sizeAndHeight = description.Split('/');
				return (sizeAndHeight[0], sizeAndHeight[1]);
			}

			if(sizeWithAnnotationRegex.IsMatch(description)) {
				var replaceRegex = new Regex(@"/D", RegexOptions.Compiled);
				var sizeName = replaceRegex.Replace(description, String.Empty).Trim();
				return (sizeName, sizeName);
			}
			return (description, description);
		}

		private void ReadDate() {
			var date = documents1C.First().Element( nsV8 + "Date")?.Value;
			if(DateTime.TryParse(date, out var resultParse))
				DocumentDate =resultParse;
		}

		private Nomenclature ParseNomenclature(string catalogReference) {
			if(catalogReference is null)
				return null;
			var catalogNomenclature = nomenclatures.FirstOrDefault(x => x.Element(nsV8 + "Ref")?.Value == catalogReference);
			var article = catalogNomenclature?.Element(nsV8 + "Артикул")?.Value;
			var nomenclatureName = catalogNomenclature?.Element(nsV8 + "НаименованиеПолное")?.Value;
			if(String.IsNullOrEmpty(article)) {
				if(!String.IsNullOrEmpty(nomenclatureName))
					if(!NotFoundNomenclatureNumbers.Contains(nomenclatureName))
						NotFoundNomenclatureNumbers.Add(nomenclatureName);
				return null;
			}

			if(UInt32.TryParse(article, out var number)) {
				var nomenclature = unitOfWork.Query<Nomenclature>().Where(x => x.Number == number).List().FirstOrDefault();
				if(nomenclature != null) return nomenclature;
				if(!NotFoundNomenclatures.Any(x => x.Article == number && x.Name == nomenclatureName))
					NotFoundNomenclatures.Add(new NotFoundNomenclature { Name = nomenclatureName, Article = number });
				return null;
			}
			if(!UnreadableArticle.Contains(article))
				UnreadableArticle.Add(article);
			return null;
		}
	}

	public class Xml1CDocumentItem 
	{
		public Nomenclature Nomenclature { get; set; }
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
