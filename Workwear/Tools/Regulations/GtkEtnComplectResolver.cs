using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using Workwear.Models.Regulations;
using Workwear.ViewModels.Regulations;
using Workwear.Views.Regulations;
using EtnNormItem = QS.Cloud.WorkwearDictionary.NormItem;
using EtnItemSIZ = QS.Cloud.WorkwearDictionary.ItemSIZ;
using EtnPeriodType = QS.Cloud.WorkwearDictionary.PeriodType;

namespace Workwear.Tools.Regulations {
	/// <summary>
	/// Показывает <see cref="EtnComplectWidgetView"/> в модальном диалоге и превращает выбор пользователя
	/// в данные для <see cref="EtnNormImportModel"/>.
	/// </summary>
	public class GtkEtnComplectResolver : IEtnComplectResolver {
		private readonly IInteractiveMessage interactiveMessage;
		private readonly IInteractiveQuestion interactiveQuestion;

		public GtkEtnComplectResolver(IInteractiveMessage interactiveMessage, IInteractiveQuestion interactiveQuestion) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new ArgumentNullException(nameof(interactiveQuestion));
		}

		public IList<EtnComplectResolvedItem> ResolveOR(EtnNormItem complect) =>
			Resolve(EtnComplectWidgetViewModel.ForOrComplect(complect, interactiveMessage));

		public IList<EtnComplectResolvedItem> ResolveNeedPeriodItems(IList<EtnItemSIZ> items) =>
			Resolve(EtnComplectWidgetViewModel.ForNeedPeriodItems(items, interactiveMessage));

		public EtnNormItem ResolveAltGroup(IList<EtnNormItem> alternatives) {
			var choice = interactiveQuestion.Question(
				alternatives.Select(x => x.ComplectName).ToArray(),
				"В приказе эти позиции указаны как альтернативные варианты - выберите один:",
				"Выбор варианта СИЗ");
			return alternatives.FirstOrDefault(x => x.ComplectName == choice);
		}

		private static IList<EtnComplectResolvedItem> Resolve(EtnComplectWidgetViewModel viewModel) {
			var selected = RunDialog(viewModel);
			return selected?.Select(ToResolvedItem).ToList();
		}

		private static EtnComplectItemNode[] RunDialog(EtnComplectWidgetViewModel viewModel) {
			EtnComplectItemNode[] result = null;

			//Явный Destroy(), а не using/Dispose() - иначе диалог не убирается с экрана до открытия следующего.
			var dialog = new Gtk.Dialog(viewModel.HeadTitle, null, Gtk.DialogFlags.Modal);
			try {
				dialog.SetDefaultSize(700, 450);
				dialog.SetPosition(Gtk.WindowPosition.Center);
				dialog.ActionArea.NoShowAll = true;
				dialog.ActionArea.Visible = false;

				var view = new EtnComplectWidgetView(viewModel);
				dialog.VBox.PackStart(view, true, true, 0);

				viewModel.AddedSelected += (s, items) => {
					result = items;
					dialog.Respond(Gtk.ResponseType.Ok);
				};
				viewModel.Canceled += (s, e) => dialog.Respond(Gtk.ResponseType.Cancel);

				view.ShowAll();
				dialog.Run();
			} finally {
				dialog.Destroy();
			}

			return result;
		}

		private static EtnComplectResolvedItem ToResolvedItem(EtnComplectItemNode node) => new EtnComplectResolvedItem {
			Name = node.Name,
			TypeName = node.Source.SizType,
			Condition = node.Source.Condition,
			Amount = node.Amount,
			PeriodCount = node.PeriodCount,
			PeriodType = node.PeriodType,
			NormItemComment = node.Comment,
			HasUndefinedPeriod = node.Source.PeriodType == EtnPeriodType.OneUse || node.Source.PeriodType == EtnPeriodType.NeedSet,
			DermalPpe = node.Source.DermalPpe,
			Unit = node.Source.Unit
		};
	}
}
