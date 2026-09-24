using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using Workwear.Models.Regulations;
using Workwear.ViewModels.Regulations;
using Workwear.Views.Regulations;
using EtnNormItem = QS.Cloud.WorkwearDictionary.Grpc.Contracts.NormItem;
using EtnItemSIZ = QS.Cloud.WorkwearDictionary.Grpc.Contracts.ItemSIZ;
using EtnPeriodType = QS.Cloud.WorkwearDictionary.Grpc.Contracts.PeriodType;

namespace Workwear.Tools.Regulations {
	/// <summary>
	/// Показывает <see cref="EtnComplectWidgetView"/> в модальном диалоге и превращает выбор пользователя
	/// в данные для <see cref="EtnNormImportModel"/>.
	/// </summary>
	public class GtkEtnComplectResolver : IEtnComplectResolver {
		private readonly IInteractiveMessage interactiveMessage;

		public GtkEtnComplectResolver(IInteractiveMessage interactiveMessage) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		public IList<EtnComplectResolvedItem> ResolveOR(EtnNormItem complect) =>
			Resolve(EtnComplectWidgetViewModel.ForOrComplect(complect, interactiveMessage));

		public IList<EtnComplectResolvedItem> ResolveNeedPeriodItems(IList<EtnItemSIZ> items) =>
			Resolve(EtnComplectWidgetViewModel.ForNeedPeriodItems(items, interactiveMessage));

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
			HasUndefinedPeriod = node.Source.PeriodType == EtnPeriodType.OneUse || node.Source.PeriodType == EtnPeriodType.NeedSet
		};
	}
}
