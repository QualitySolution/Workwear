using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.ViewModels;
using Workwear.Domain.Regulations;
using Workwear.Models.Regulations;
using EtnNormItem = QS.Cloud.WorkwearDictionary.NormItem;
using EtnItemSIZ = QS.Cloud.WorkwearDictionary.ItemSIZ;
using EtnPeriodType = QS.Cloud.WorkwearDictionary.PeriodType;

namespace Workwear.ViewModels.Regulations {
	/// <summary>
	/// Выбор позиций для добавления в норму отдельными строками: пользователь может отредактировать позиции перед добавлением.
	/// </summary>
	public class EtnComplectWidgetViewModel : ViewModelBase {
		private const int CombinedNameMaxLength = 800;

		private readonly IInteractiveMessage interactiveMessage;

		public EtnComplectWidgetViewModel(
			string headComment,
			IList<EtnComplectItemNode> items,
			bool addAllItems,
			IInteractiveMessage interactiveMessage)
		{
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			Items = items ?? throw new ArgumentNullException(nameof(items));
			AddAllItems = addAllItems;

			HeadTitle = "Добавление комплекта";
			HeadComment = headComment;
		}

		/// <summary>
		/// Комплект "или": первой строкой идёт объединённый вариант затем каждая позиция отдельно.
		/// </summary>
		public static EtnComplectWidgetViewModel ForOrComplect(EtnNormItem complect, IInteractiveMessage interactiveMessage) {
			var itemNodes = complect.Items.Select(x => new EtnComplectItemNode(x)).ToList();
			var items = new List<EtnComplectItemNode> { EtnComplectItemNode.CreateOrCombined(itemNodes, CombinedNameMaxLength) };
			items.AddRange(itemNodes);
			return new EtnComplectWidgetViewModel("Выберите используемый у вас вариант:", items, addAllItems: false, interactiveMessage);
		}

		/// <summary>
		/// Позиции с неопределённым сроком выдачи (period_type "разовое использование"/"по необходимости")
		/// </summary>
		public static EtnComplectWidgetViewModel ForNeedPeriodItems(IList<EtnItemSIZ> items, IInteractiveMessage interactiveMessage) {
			var itemNodes = items.Select(x => new EtnComplectItemNode(x)).ToList();
			return new EtnComplectWidgetViewModel("Проставьте количество и сроки эксплуатации для всех позиций:", itemNodes, addAllItems: true, interactiveMessage) {
				HeadTitle = "Количество и сроки эксплуатации"
			};
		}

		public IList<EtnComplectItemNode> Items { get; }

		/// <summary>
		/// Все позиции требуют заполнения и добавляются вместе, независимо от выделения строк.
		/// </summary>
		public bool AddAllItems { get; }

		public bool CanAddAllItems => Items.Count > 0 && !Items.Any(IsInvalid);

		private string headTitle;
		public virtual string HeadTitle {
			get => headTitle;
			set => SetField(ref headTitle, value);
		}

		private string headComment;
		public virtual string HeadComment {
			get => headComment;
			set => SetField(ref headComment, value);
		}

		/// <summary>
		/// Пользователь подтвердил добавление выбранных позиций отдельными строками нормы.
		/// </summary>
		public event EventHandler<EtnComplectItemNode[]> AddedSelected;

		/// <summary>
		/// Пользователь отказался добавлять позиции.
		/// </summary>
		public event EventHandler Canceled;

		/// <summary>
		/// Проверяет количество и срок всех позиций в режиме заполнения или выбранного варианта комплекта.
		/// </summary>
		public void AddSelected(EtnComplectItemNode[] selected) {
			if(AddAllItems)
				selected = Items.ToArray();
			if(selected == null || selected.Length == 0)
				return;

			var invalidItem = selected.FirstOrDefault(IsInvalid);
			if(invalidItem != null) {
				Warn(invalidItem);
				return;
			}

			AddedSelected?.Invoke(this, selected);
		}

		public void Cancel() => Canceled?.Invoke(this, EventArgs.Empty);

		private static bool IsInvalid(EtnComplectItemNode item) {
			if(!(item.Amount > 0) || item.PeriodType == null)
				return true;
			//Для "до износа" период не требуется.
			return item.PeriodType != NormPeriodType.Wearout && !(item.PeriodCount > 0);
		}

		private void Warn(EtnComplectItemNode item) {
			interactiveMessage.ShowMessage(ImportanceLevel.Warning,
				$"У позиции «{item.Name}» должны быть заполнены и количество, и период выдачи.\n",
				"Нельзя добавить");
		}
	}

	/// <summary>
	/// Позиция для отображения в <see cref="EtnComplectWidgetViewModel"/>, редактируемая пользователем
	/// перед добавлением в норму.
	/// </summary>
	public class EtnComplectItemNode : PropertyChangedBase {
		public EtnComplectItemNode(EtnItemSIZ source) {
			Source = source ?? throw new ArgumentNullException(nameof(source));
			Name = EtnNormImportModel.FormatDermalName(source.SizName, source.DermalPpe, source.Amount, source.Unit);

			Comment = source.PeriodSpecial;
			if(source.PeriodType == EtnPeriodType.NeedSet || source.PeriodType == EtnPeriodType.OneUse) {
			} else if(source.DermalPpe) {
				//Объём/масса вынесено в название - "Количество" тут значит "сколько упаковок выдаётся за период".
				amount = 1;
				periodCount = source.PeriodCount > 0 ? source.PeriodCount : (int?)null;
				periodType = EtnNormImportModel.MapPeriodType(source.PeriodType);
			} else {
				amount = source.Amount > 0 ? source.Amount : (int?)null;
				periodCount = source.PeriodCount > 0 ? source.PeriodCount : (int?)null;
				periodType = EtnNormImportModel.MapPeriodType(source.PeriodType);
			}
		}

		private EtnComplectItemNode(string name, EtnItemSIZ representativeSource, int? amount, int? periodCount, NormPeriodType? periodType, string comment) {
			Name = name;
			Source = representativeSource;
			IsCombined = true;
			Comment = comment;
			this.amount = amount;
			this.periodCount = periodCount;
			this.periodType = periodType;;
		}

		/// <summary>
		/// Строка "все варианты через или"
		/// </summary>
		public static EtnComplectItemNode CreateOrCombined(IList<EtnComplectItemNode> items, int maxNameLength) {
			var joined = String.Join(" или ", items.Select(x => x.Name));
			if(joined.Length > maxNameLength)
				joined = joined.Substring(0, maxNameLength - 1) + "…";

			var representative = items.First();
			var comment = String.Join(". ", items.Select(x => x.Comment).Where(x => !String.IsNullOrWhiteSpace(x)).Distinct());
			return new EtnComplectItemNode(joined, representative.Source, representative.Amount, representative.PeriodCount, representative.PeriodType, comment);
		}

		public EtnItemSIZ Source { get; }
		public string Name { get; }
		public string Comment { get; }

		/// <summary>
		/// Объединённый вариант ("все через или") - выделяется в виджете
		/// </summary>
		public bool IsCombined { get; }

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
	}
}
