using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using QSOrmProject;
using workwear.Domain.Stock;
using Workwear.Measurements;
using workwear.ViewModels.Stock;
using IdToStringConverter = Gamma.Binding.Converters.IdToStringConverter;

namespace workwear.Views.Stock
{
	public partial class IncomeView : EntityDialogViewBase<IncomeViewModel, Income>
	{
		private readonly IncomeViewModel viewModel;
		public IncomeView(IncomeViewModel viewModel): base(viewModel)
		{
			this.viewModel = viewModel;
			Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			buttonAdd.Clicked += AddItem;
			buttonDel.Clicked += DelItem;
			
			ylabelId.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter())
				.InitializeFromSource();
			ylabelCreatedBy.Binding.AddFuncBinding(Entity, e => e.CreatedbyUser.Name,w => w.LabelProp)
				.InitializeFromSource();
			ydateDoc.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();
			ytextComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			labelSum.Binding.AddBinding(viewModel, v => v.Sum, w => w.LabelProp).InitializeFromSource();
			
			entityWarehouseIncome.Binding
				.AddBinding(viewModel, vm => vm.ShowWarehouses, v => v.Visible).InitializeFromSource();

			ytreeItems.ColumnsConfig = ColumnsConfigFactory.Create<IncomeItem>()
				.AddColumn ("Наименование").AddTextRenderer (e => e.Nomenclature.Name)
				.AddColumn("Сертификат").AddTextRenderer(e => e.Certificate).Editable()
				.AddColumn("Размер").MinWidth(60)
				.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
				.DynamicFillListFunc(x => SizeService.GetSize(viewModel.UoW, x.Nomenclature.Type.SizeType))
				.AddSetter((c,n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
				.AddColumn("Рост").MinWidth(70)
				.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
				.DynamicFillListFunc(x => SizeService.GetSize(viewModel.UoW, x.Nomenclature.Type.HeightType))
				.AddSetter((c,n) => c.Editable = n.Nomenclature?.Type?.HeightType != null)
				.AddColumn ("Процент износа").AddNumericRenderer(e => e.WearPercent, new MultiplierToPercentConverter())
				.Editing (new Adjustment(0,0,999,1,10,0)).WidthChars(6).Digits(0)
				.AddTextRenderer (e => "%", expand: false)
				.AddColumn ("Количество").AddNumericRenderer (e => e.Amount)
				.Editing(new Adjustment(0, 0, 100000, 1, 10, 1)).WidthChars(8)
				.AddTextRenderer (e => e.Nomenclature.Type.Units.Name)
				.AddColumn ("Стоимость").AddNumericRenderer (e => e.Cost)
				.Editing(new Adjustment(0,0,100000000,100,1000,0)).Digits (2).WidthChars(12)
				.AddColumn("Сумма").AddNumericRenderer(x => x.Total).Digits(2)
				.AddColumn("Бухгалтерский документ").Tag(ColumnTags.BuhDoc).AddTextRenderer(e => e.BuhDocument).Editable()
				.Finish ();
			ytreeItems.ItemsDataSource = Entity.ObservableItems;
		}
		private void AddItem(object sender, EventArgs eventArgs) => ViewModel.AddItems();
		private void DelItem(object sender, EventArgs eventArgs) => 
			ViewModel.DelItems(ytreeItems.GetSelectedObject<IncomeItem>());
		private enum ColumnTags {
			BuhDoc
		}
	}
}
