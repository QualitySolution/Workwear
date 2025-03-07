using System;
using QS.Views.Dialog;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
	public partial class BarcodingView : EntityDialogViewBase<BarcodingViewModel, Barcoding> {
		public BarcodingView(BarcodingViewModel viewModel) : base(viewModel) {
			Build();
			ConfigureDlg();
			ConfigureItems();
			CommonButtonSubscription();
		}

		private void ConfigureDlg() {
			
			entryId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DocNumberText, w => w.Text)
				.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive)
				.InitializeFromSource();
			checkAuto.Binding.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active).InitializeFromSource(); 
			
			ylabelUser.Binding
				.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp).InitializeFromSource();
			ydateDoc.Binding
				.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();
			labelSum.Binding
				.AddBinding(ViewModel, vm => vm.Total, w => w.LabelProp).InitializeFromSource();

			ytreeItems.Selection.Changed += Items_Selection_Changed;
			ybuttonAdd.Clicked += OnButtonAddClicked;
			ybuttonDel.Clicked += OnButtonDelClicked;
			buttonPrint.Clicked += OnButtonPrintClicked;
		}

		private void ConfigureItems()
		{
		/*	ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<BarcodingItem> ()
					.AddColumn ("Сотрудник").Resizable().AddReadOnlyTextRenderer(e => e.Employee.FullName)
					.AddColumn("Номер\nкарточки").AddTextRenderer(e => e.EmployeeNumber)
					.AddColumn ("Номенклатура").Resizable().AddReadOnlyTextRenderer(e => e?.Nomenclature?.Name).WrapWidth(1000)
					.AddColumn ("Выдано").AddReadOnlyTextRenderer(e => e.Amount.ToString())
					.AddColumn ("Дата\nвыдачи").AddReadOnlyTextRenderer(e => e.IssueDate?.ToShortDateString() ?? "")
					.AddColumn ("Выдано до").AddReadOnlyTextRenderer(e => e.ExpiryByNormBefore?.ToShortDateString() ??  "до износа")
					.AddColumn ("% износа на\nдату выдачи").AddReadOnlyTextRenderer((e => ((int)(e.WearPercentBefore * 100))
						.Clamp(0, 100) + "%"))
					.AddColumn ("Установить\n% износа").AddNumericRenderer (e => e.WearPercentAfter, new MultiplierToPercentConverter())
						.Editing (new Adjustment(0,0,999,1,10,0)).WidthChars(6).Digits(0)
						.AddTextRenderer (e => "%", expand: false)
					.AddColumn("Списать").AddToggleRenderer(e => e.Writeoff).Editing()
					.AddColumn ("Продлить").AddDateRenderer(e => e.ExpiryByNormAfter).Editable()
					.AddColumn("Отметка об износе").AddTextRenderer(e => e.Cause).WrapWidth(800).Editable()
					.Finish ();
			ytreeItems.Binding
				.AddBinding(Entity, vm => vm.Items, w => w.ItemsDataSource)
				.InitializeFromSource();
				*/
		}
		
		private void OnButtonDelClicked(object sender, EventArgs e) => ViewModel.DeleteItem(ytreeItems.GetSelectedObject<BarcodingItem>());
		private void OnButtonAddClicked(object sender, EventArgs e) => ViewModel.AddItems();
		private void OnButtonPrintClicked(object sender, EventArgs e) => ViewModel.Print();
		private void Items_Selection_Changed(object sender, EventArgs e){
			ybuttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}
	}
}

