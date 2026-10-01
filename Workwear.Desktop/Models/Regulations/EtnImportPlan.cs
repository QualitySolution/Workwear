using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Workwear.Domain.Regulations;
using EtnComplectType = QS.Cloud.WorkwearDictionary.ComplectType;
using EtnGetNormResponse = QS.Cloud.WorkwearDictionary.GetNormResponse;
using EtnItemSIZ = QS.Cloud.WorkwearDictionary.ItemSIZ;
using EtnNormItem = QS.Cloud.WorkwearDictionary.NormItem;
using EtnPeriodType = QS.Cloud.WorkwearDictionary.PeriodType;

namespace Workwear.Models.Regulations {
	/// <summary>
	/// План импорта из ЕТН - все строки будущей нормы сразу, с выбором варианта прямо в строке там где приказ даёт альтернативу.
	/// </summary>
	public class EtnImportPlan {
		private readonly List<EtnImportBlock> blocks = new List<EtnImportBlock>();

		/// <summary>
		/// Строки будущей нормы. Пересобираются по месту при смене варианта в блоке.
		/// </summary>
		public IObservableList<EtnImportPlanRow> Rows { get; } = new ObservableList<EtnImportPlanRow>();

		public IReadOnlyList<EtnImportBlock> Blocks => blocks;

		/// <summary>
		/// Строки, которые нельзя добавить в норму - не проставлено количество или срок носки.
		/// </summary>
		public IEnumerable<EtnImportPlanRow> IncompleteRows => Rows.Where(x => !x.IsComplete);

		#region Построение плана

		/// <summary>
		/// Добавляет в план строки одного выбранного узла дерева. 
		/// </summary>
		public void AddFrom(EtnGetNormResponse response, int appendixNumber, int sourceKey) {
			if(response == null)
				throw new ArgumentNullException(nameof(response));

			var sources = response.Norms.GroupBy(x => x.NormId).ToDictionary(g => g.Key, g => g.First());

			foreach(var byNorm in response.Items.GroupBy(x => x.NormId)) {
				sources.TryGetValue(byNorm.Key, out var source);
				var paragraph = $"п.{source?.NumberNorm} Приложение {appendixNumber} приказа №767Н от 29.10.2021";

				foreach(var alternatives in SplitIntoBlocks(byNorm))
					AddBlock(new EtnImportBlock(this, alternatives, paragraph, source?.NormName, sourceKey));
			}
		}

		/// <summary>
		/// Убирает из плана всё, что пришло от снятого узла. 
		/// </summary>
		public void RemoveSource(int sourceKey) {
			for(var i = Rows.Count - 1; i >= 0; i--)
				if(Rows[i].Block.SourceKey == sourceKey)
					Rows.RemoveAt(i);

			blocks.RemoveAll(x => x.SourceKey == sourceKey);
		}

		/// <summary>
		/// Блоки, соединённые в приказе словом "или" на уровне блоков, помечены общим ненулевым
		/// <see cref="EtnNormItem.AltGroup"/> и образуют один блок выбора. 
		/// </summary>
		private static IEnumerable<IList<EtnNormItem>> SplitIntoBlocks(IEnumerable<EtnNormItem> complects) {
			var groups = new List<IList<EtnNormItem>>();
			var byAltGroup = new Dictionary<int, List<EtnNormItem>>();

			foreach(var complect in complects) {
				if(complect.AltGroup == 0) {
					groups.Add(new List<EtnNormItem> { complect });
					continue;
				}
				if(!byAltGroup.TryGetValue(complect.AltGroup, out var group)) {
					group = new List<EtnNormItem>();
					byAltGroup.Add(complect.AltGroup, group);
					groups.Add(group);
				}
				group.Add(complect);
			}
			return groups;
		}

		private void AddBlock(EtnImportBlock block) {
			blocks.Add(block);
			foreach(var row in block.BuildRows())
				Rows.Add(row);
		}

		#endregion

		/// <summary>
		/// Пользователь сменил вариант в блоке.
		/// Оновление без потери уже вручную внесённого.
		/// </summary>
		internal void RebuildRowsOf(EtnImportBlock block) {
			var existing = new List<int>();
			for(var i = 0; i < Rows.Count; i++)
				if(Rows[i].Block == block)
					existing.Add(i);

			var rebuilt = block.BuildRows().ToList();

			if(existing.Count == rebuilt.Count) {
				for(var i = 0; i < rebuilt.Count; i++)
					Rows[existing[i]].ResetFrom(rebuilt[i]);
				return;
			}

			var firstIndex = existing.Count > 0 ? existing[0] : Rows.Count;
			for(var i = existing.Count - 1; i >= 0; i--)
				Rows.RemoveAt(existing[i]);

			foreach(var row in rebuilt)
				Rows.Insert(firstIndex++, row);
		}
	}

	/// <summary>
	/// Блок приказа - единица выбора. У блока без альтернатив вариант один и колонка выбора в строке не редактируется.
	/// </summary>
	public class EtnImportBlock {
		private readonly EtnImportPlan plan;
		private readonly IList<EtnNormItem> complects;

		internal EtnImportBlock(EtnImportPlan plan, IList<EtnNormItem> complects, string normParagraph, string sourceName, int sourceKey) {
			this.plan = plan;
			this.complects = complects;
			NormParagraph = normParagraph;
			SourceName = sourceName;
			SourceKey = sourceKey;
			IsAdditional = complects[0].IsAdditional;
			BlockName = complects[0].ComplectName;

			Variants = BuildVariants(complects);
			variant = Variants.FirstOrDefault();
		}

		public string NormParagraph { get; }

		/// <summary>
		/// Название узла-источника потребности (опасное событие / объект загрязнения).
		/// </summary>
		public string SourceName { get; }

		/// <summary>
		/// Id узла, который отметил пользователь (не обязательно узел самой строки - при include_children строки приезжают от потомков).
		/// </summary>
		internal int SourceKey { get; }

		public string BlockName { get; }
		public bool IsAdditional { get; }

		public IList<EtnImportVariant> Variants { get; }

		/// <summary>
		/// Есть ли у пользователя выбор. Условие включает редактирование колонки варианта.
		/// </summary>
		public bool HasChoice => Variants.Count > 1;

		private EtnImportVariant variant;
		public EtnImportVariant Variant {
			get => variant;
			set {
				if(ReferenceEquals(variant, value) || value == null)
					return;
				variant = value;
				plan.RebuildRowsOf(this);
			}
		}

		internal IEnumerable<EtnImportPlanRow> BuildRows() {
			if(Variant == null)
				yield break;

			//Комбинированный вариант - одна номенклатура с составным названием ("А или Б или В")
			if(Variant.IsCombined) {
				yield return new EtnImportPlanRow(this, Variant.Items[0], Variant.Name);
				yield break;
			}

			foreach(var item in Variant.Items)
				yield return new EtnImportPlanRow(this, item,
					EtnNormImportModel.FormatDermalName(item.SizName, item.DermalPpe, item.Amount, item.Unit));
		}

		private const int CombinedNameMaxLength = 800;

		private static IList<EtnImportVariant> BuildVariants(IList<EtnNormItem> complects) {
			var result = new List<EtnImportVariant>();
			var prefixWithBlockName = complects.Count > 1;

			foreach(var complect in complects)
				result.AddRange(VariantsOf(complect, prefixWithBlockName));

			return result;
		}

		private static IEnumerable<EtnImportVariant> VariantsOf(EtnNormItem complect, bool prefixWithBlockName) {
			var itemNames = complect.Items
				.Select(x => EtnNormImportModel.FormatDermalName(x.SizName, x.DermalPpe, x.Amount, x.Unit))
				.ToList();

			//"Или" внутри блока: пользователь выбирает одну позицию, либо объединённую номенклатуру.
			if(complect.ComplectType == EtnComplectType.Or && complect.Items.Count > 1) {
				var combined = String.Join(" или ", itemNames);
				if(combined.Length > CombinedNameMaxLength)
					combined = combined.Substring(0, CombinedNameMaxLength - 1) + "…";

				yield return new EtnImportVariant(Prefix(combined, complect, prefixWithBlockName), complect.Items, isCombined: true);
				for(var i = 0; i < complect.Items.Count; i++)
					yield return new EtnImportVariant(
						Prefix(itemNames[i], complect, prefixWithBlockName),
						new[] { complect.Items[i] });
				yield break;
			}

			//"И" и одиночный комплект выбора не дают - все позиции идут в норму вместе. Пока не нашёл где нужно иначе.
			var name = !String.IsNullOrWhiteSpace(complect.ComplectName)
				? complect.ComplectName
				: String.Join(", ", itemNames);
			yield return new EtnImportVariant(name, complect.Items);
		}

		private static string Prefix(string name, EtnNormItem complect, bool prefixWithBlockName) =>
			prefixWithBlockName && !String.IsNullOrWhiteSpace(complect.ComplectName)
				? $"{complect.ComplectName.Trim()}: {name}"
				: name;

		/// <summary>
		/// Формирует комментарий строки из особого периода выбранной позиции. Для объединённого
		/// варианта "или" собирает особые периоды всех его позиций, для комплекта "и" добавляет
		/// название комплекта. Пустые значения и повторы исключаются; без текста возвращает null.
		/// </summary>
		internal string RowComment(EtnImportPlanRow row) {
			var complect = complects.FirstOrDefault(c => c.Items.Contains(row.Source)) ?? complects[0];
			var items = Variant.IsCombined ? Variant.Items : new[] { row.Source };
			var comments = items.Select(x => x.PeriodSpecial);
			if(complect.ComplectType == EtnComplectType.And)
				comments = new[] { complect.ComplectName }.Concat(comments);
			var comment = String.Join(". ", comments.Where(x => !String.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct());
			return String.IsNullOrEmpty(comment) ? null : comment;
		}
	}

	/// <summary>
	/// Вариант выбора внутри блока: одна позиция, объединённая номенклатура "или", либо весь состав комплекта "и".
	/// </summary>
	public class EtnImportVariant {
		public EtnImportVariant(string name, IList<EtnItemSIZ> items, bool isCombined = false) {
			Name = name;
			Items = items;
			IsCombined = isCombined;
		}

		public string Name { get; }
		public IList<EtnItemSIZ> Items { get; }

		/// <summary>
		/// Объединённый вариант "все названия через или" - одна номенклатура вместо нескольких строк.
		/// </summary>
		public bool IsCombined { get; }

		public override string ToString() => Name;
	}

	/// <summary>
	/// Строка будущей нормы.
	/// </summary>
	public class EtnImportPlanRow : PropertyChangedBase {
		internal EtnImportPlanRow(EtnImportBlock block, EtnItemSIZ source, string name) {
			Block = block;
			Apply(source, name);
		}

		/// <summary>
		/// Перенастраивает строку под другой вариант блока, не заменяя её в списке. 
		/// </summary>
		internal void ResetFrom(EtnImportPlanRow other) {
			Apply(other.Source, other.Name);
			FirePropertyChanged();
		}

		private void Apply(EtnItemSIZ source, string name) {
			Source = source;
			Name = name;

			NeedsPeriod = source.PeriodType == EtnPeriodType.OneUse || source.PeriodType == EtnPeriodType.NeedSet;
			if(NeedsPeriod) {
				amount = null;
				periodCount = null;
				periodType = null;
				return;
			}

			//Для дерматологических СИЗ объём/масса уже в названии.
			amount = source.DermalPpe ? 1 : (source.Amount > 0 ? source.Amount : (int?)null);
			periodCount = source.PeriodCount > 0 ? source.PeriodCount : (int?)null;
			periodType = EtnNormImportModel.MapPeriodType(source.PeriodType);
		}

		internal EtnImportBlock Block { get; }
		public EtnItemSIZ Source { get; private set; }
		public string Name { get; private set; }

		/// <summary>
		/// В приказе у позиции не определён срок выдачи - пользователь обязан проставить его сам.
		/// </summary>
		public bool NeedsPeriod { get; private set; }

		public string NormParagraph => Block.NormParagraph;
		public string SourceName => Block.SourceName;
		public bool IsAdditional => Block.IsAdditional;
		public string Comment => Block.RowComment(this);

		/// <summary>
		/// Выбор варианта блока. Свойство проброшено для биндинга.
		/// </summary>
		public EtnImportVariant Variant {
			get => Block.Variant;
			set => Block.Variant = value;
		}

		public IList<EtnImportVariant> Variants => Block.Variants;
		public bool HasChoice => Block.HasChoice;

		private int? amount;
		public virtual int? Amount {
			get => amount;
			set => SetField(ref amount, value);
		}

		private int? periodCount;
		public virtual int? PeriodCount {
			get => periodCount;
			set => SetField(ref periodCount, value);
		}

		private NormPeriodType? periodType;
		public virtual NormPeriodType? PeriodType {
			get => periodType;
			set => SetField(ref periodType, value);
		}

		/// <summary>
		/// Строку можно добавить в норму. "До износа" срок не требует - там период не применяется.
		/// </summary>
		public bool IsComplete =>
			amount > 0 && periodType != null
			&& (periodType == NormPeriodType.Wearout || periodCount > 0);
	}
}
