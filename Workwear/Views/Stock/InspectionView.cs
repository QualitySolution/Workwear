using System;
using Gamma.Binding.Converters;
using QS.Views.Dialog;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock {
	public partial class InspectionView : EntityDialogViewBase<InspectionViewModel, Inspection> {
		public InspectionView(InspectionViewModel viewModel) : base(viewModel) {
			Build();
			ConfigureDlg();
			ConfigureItems();
			CommonButtonSubscription();
		}

		private void ConfigureDlg() {
			ylabelId.Binding
				.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter())
				.InitializeFromSource();
			ylabelUser.Binding
				.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp)
				.InitializeFromSource();
			ydateDoc.Binding
				.AddBinding(Entity, e => e.Date, w => w.Date)
				.InitializeFromSource();
			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();
			labelSum.Binding
				.AddBinding(ViewModel, vm => vm.Total, w => w.LabelProp)
				.InitializeFromSource();
			ybuttonDel.Binding
				.AddBinding(ViewModel, vm => vm.DelSensitive, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonAdd.Binding
				.AddBinding(ViewModel, vm => vm.AddEmployeeSensitive, w => w.Sensitive)
				.InitializeFromSource();
			ybuttonDel.Clicked += OnButtonDelClicked;
			ybuttonAdd.Clicked += OnButtonAddClicked;

		}
		
				private void ConfigureItems()
		{
			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<InspectionItem> ()
					.AddColumn ("Сотрудник").AddReadOnlyTextRenderer(e => e.Employee.FullName)
					.AddColumn ("Номенклатура").AddReadOnlyTextRenderer(e => e?.Nomenclature?.Name)
					.AddColumn ("Дата").AddReadOnlyTextRenderer(e => e.IssuedDate.ToShortDateString())
					.Finish ();
			
			ytreeItems.Binding
				.AddBinding(Entity, vm => vm.ObservableItems, w => w.ItemsDataSource)
				.InitializeFromSource();
		}

		
		private void OnButtonDelClicked(object sender, EventArgs e) => ViewModel.DeleteItem(ytreeItems.GetSelectedObject<InspectionItem>());
		private void OnButtonAddClicked(object sender, EventArgs e) => ViewModel.AddItems();
		
	}
}
