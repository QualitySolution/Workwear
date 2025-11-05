using System;
using Gtk;
using QS.Views.Dialog;
using Workwear.Domain.Supply;
using Workwear.ViewModels.Supply;

namespace Workwear.Views.Supply {
	public partial class ShipmentView : EntityDialogViewBase<ShipmentViewModel,Shipment> {
		public ShipmentView(ShipmentViewModel viewModel): base(viewModel) {
			this.Build();
			ConfigureDlg();
			ConfigureItems();
			CommonButtonSubscription();
		}
		private void ConfigureDlg() {
			ylabelNumber.Binding.AddBinding(ViewModel, v => v.DocID, w => w.LabelProp)
				.InitializeFromSource();
			datePeriod.Binding.AddSource(ViewModel)
				.AddBinding(v => v.StartPeriod, w => w.StartDateOrNull)
				.AddBinding(v => v.EndPeriod,w => w.EndDateOrNull)
				.InitializeFromSource();
			ylabelCreatedBy.Binding.AddFuncBinding(ViewModel, v => v.CreatedByUser!=null? v.CreatedByUser.Name: null, w => w.LabelProp)
				.InitializeFromSource();
			ytextComment.Binding.AddBinding(ViewModel, v => v.DocComment,w => w.Buffer.Text)
				.InitializeFromSource();
			ylabelAmount.Binding.AddBinding(ViewModel, v => v.Total, w => w.LabelProp)
				.InitializeFromSource();
			ybuttonAdd.Binding.AddBinding(ViewModel, vm => vm.CanAddItem, w => w.Sensitive).InitializeFromSource();
			ybuttonDel.Binding.AddBinding(ViewModel, vm => vm.CanRemoveItem, w => w.Sensitive).InitializeFromSource();
			buttonToOrder.Binding.AddBinding(ViewModel, vm => vm.CanToOrder, w => w.Sensitive).InitializeFromSource();
			yenumcomboboxStatus.ItemsEnum=typeof(ShipmentStatus);
			yenumcomboboxStatus.Binding
				.AddBinding(Entity,v => v.Status,w => w.SelectedItem)
				.InitializeFromSource();
			ybuttonSendEmail.Binding
				.AddBinding(ViewModel, vm => vm.CanSandEmail, w => w.Visible).InitializeFromSource();
			ybuttonPrint.Clicked += (s,e) => ViewModel.Print();
			ybuttonAddSizes.Binding.AddBinding(ViewModel, vm => vm.CanAddSize, w => w.Sensitive).InitializeFromSource();
		}

		private void ConfigureItems() {
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ShipmentItem>()
				.AddColumn("Ном. №")
					.AddReadOnlyTextRenderer(x => x.Nomenclature?.Number)
				.AddColumn("Наименование").Resizable()
					.AddTextRenderer(e => e.ItemName).WrapWidth(700)
					.AddSetter((w, item) => w.Foreground = item.Nomenclature != null ? "black" : "red")
				.AddColumn("Размер")
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.GetSizeVariants(x))
					.AddSetter((c, n) => c.Editable = n.WearSizeType != null)
				.AddColumn("Рост").MinWidth(70)
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => ViewModel.GetHeightVariants(x))
					.AddSetter((c, n) => c.Editable = n.HeightType != null)
				.AddColumn("Запрошено")
					.AddNumericRenderer(e => e.Requested)
					.Editing(new Adjustment(0, 0, 100000, 1, 10, 1), ViewModel.CanEditRequested ).WidthChars(8)
					.AddReadOnlyTextRenderer(e => e.Units?.Name)
				.AddColumn("|").AddReadOnlyTextRenderer(e => "|") //  Сепаратор
				.AddColumn("Заказано")
					.AddNumericRenderer(e => e.Ordered)
					.Editing(new Adjustment(0, 0, 100000, 1, 10, 1), ViewModel.CanEditOrdered ).WidthChars(8)
					.AddReadOnlyTextRenderer(e => e.Units?.Name)
				.AddColumn("Стоимость")
					.AddNumericRenderer(e => e.Cost)
					.Editing(new Adjustment(0, 0, 100000000, 100, 1000, 0)).Digits(2).WidthChars(12)
				.AddColumn("Сумма")
					.AddNumericRenderer(x => x.TotalRequested).Digits(2)
                .AddColumn("Причина расхождения").Visible(ViewModel.CanEditDiffCause)
                    .AddTextRenderer(e => e.DiffCause)
                    .Editable()
				.AddColumn("|").AddReadOnlyTextRenderer(e => "|") //  Сепаратор
				.AddColumn("Получено")
					.AddReadOnlyTextRenderer(e => e.Received.ToString() + ' ' + e.Units?.Name)
				.AddColumn("Не доставлено")
					.AddReadOnlyTextRenderer(e => $"{e.OrderedNotReceived} {e.Units?.Name}")
				.AddColumn("Комментарий")
					.AddTextRenderer(e => e.Comment)
					.Editable()

				.Finish();
			
			ytreeItems.Selection.Changed += ytreeItems_Selection_Changed;
			ytreeItems.Selection.Mode = SelectionMode.Multiple;
			ytreeItems.ItemsDataSource = ViewModel.Items;
		}
		
		private void ytreeItems_Selection_Changed(object sender, EventArgs e) {
			ViewModel.SelectedItems = ytreeItems.GetSelectedObjects<ShipmentItem>();
		}
		protected void OnYbuttonAddClicked(object sender, EventArgs e) =>
			ViewModel.AddItem();
		protected void OnYbuttonDelClicked(object sender, EventArgs e) =>
			ViewModel.DeleteItems(ytreeItems.GetSelectedObjects<ShipmentItem>());

		protected void OnYbuttonSendEmailClicked(object sender, EventArgs e) =>
			ViewModel.SendMessageForBuyer();

		protected void OnButtonToOrderClicked(object sender, EventArgs e) {
			ViewModel.ToOrderItems();
		}
		
		protected void OnYbuttonAddSizesClicked(object sender, EventArgs e) {
			ViewModel.AddSize(ytreeItems.GetSelectedObjects<ShipmentItem>());
		}
	}
}


