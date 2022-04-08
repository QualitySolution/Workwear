using System;
using System.Linq;
using Gtk;
using QS.Views.Dialog;
using workwear.Domain.Stock;
using workwear.Tools.IdentityCards;
using workwear.ViewModels.Stock;
using Workwear.Measurements;

namespace workwear.Views.Stock
{
	public partial class IssueByIdentifierView : DialogViewBase<IssueByIdentifierViewModel>
	{
		private IssueByIdentifierViewModel viewModel;
		public IssueByIdentifierView(IssueByIdentifierViewModel viewModel) : base(viewModel)
		{
			this.viewModel = viewModel;
			Build();
			#region Считыватель
			ylabelStatus.Binding.AddSource(ViewModel)
				.AddBinding(v => v.CurrentState, w => w.LabelProp)
				.InitializeFromSource();
			eventboxStatus.Binding
				.AddBinding(ViewModel, v => v.CurrentStateColor, w => w.BackgroundColor)
				.InitializeFromSource();

			comboDevice.SetRenderTextFunc<DeviceInfo>(x => x.Title);
			comboDevice.Binding.AddSource(viewModel)
				.AddBinding(v => v.Devices, w => w.ItemsList)
				.AddBinding(v => v.SelectedDevice, w => w.SelectedItem)
				.InitializeFromSource();
			checkSettings.Binding
				.AddBinding(viewModel, v => v.ShowSettings, w => w.Active)
				.InitializeFromSource();
			tableSettings.Binding
				.AddBinding(viewModel, v => v.ShowSettings, w => w.Visible)
				.InitializeFromSource();

			ytreeviewCardTypes.CreateFluentColumnsConfig<CardType>()
				.AddColumn("Вкл.").AddToggleRenderer(x => x.Active).Editing()
				.AddColumn("Тип карты").AddTextRenderer(x => x.Title)
				.Finish();
			ytreeviewCardTypes.ItemsDataSource = ViewModel.CardFamilies;
			#endregion
			#region Настройки
			entityWarehouse.ViewModel = ViewModel.WarehouseEntryViewModel;
			#endregion
			#region Выдача
			buttonCancel.Binding
				.AddBinding(ViewModel, v => v.VisibleCancelButton, w => w.Visible)
				.InitializeFromSource();
			labelFIO.Binding
				.AddFuncBinding(ViewModel, v => 
					$"<span foreground=\"Dark Green\" size=\"28000\">{v.EmployeeFullName}</span>", w => w.LabelProp)
				.InitializeFromSource();

			labelRecommendedActions.Binding
				.AddFuncBinding(ViewModel, v => 
					$"<span foreground=\"blue\" size=\"28000\">{v.RecommendedActions}</span>", w => w.LabelProp)
				.InitializeFromSource();
			hboxRecommendedActions.Binding
				.AddBinding(ViewModel, v => v.VisibleRecommendedActions, w => w.Visible)
				.InitializeFromSource();

			treeItems.Binding
				.AddBinding(ViewModel, v => v.ObservableItems, w => w.ItemsDataSource)
				.InitializeFromSource();

			eventboxSuccessfully.Binding
				.AddBinding(ViewModel, v => v.VisibleSuccessfully, w => w.Visible)
				.InitializeFromSource();
			eventboxSuccessfully.BackgroundColor = "Pale Green";
			labelSuccessfully.Binding
				.AddFuncBinding(ViewModel, v => 
					$"<span foreground=\"Dark Green\" size=\"28000\">{v.SuccessfullyText}</span>", w => w.LabelProp)
				.InitializeFromSource();
			#endregion

			CreateTable();
		}

		#region private
		void CreateTable()
		{
			var fontDesc = new Pango.FontDescription();
			fontDesc.Size = Convert.ToInt32(16 * Pango.Scale.PangoScale);
			treeItems.ModifyFont(fontDesc);
			treeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ExpenseItem>()
				.AddColumn("Номенаклатуры нормы")
				.AddTextRenderer(node => node.ProtectionTools != null ? node.ProtectionTools.Name : "")
				.AddColumn("Номенклатура").AddComboRenderer(x => x.StockBalanceSetter)
				.SetDisplayFunc(x => x.Nomenclature?.Name)
					.SetDisplayListFunc(x => x.StockPosition.Title + " - " + x.Nomenclature.GetAmountAndUnitsText(x.Amount))
					.DynamicFillListFunc(x => x.EmployeeCardItem.BestChoiceInStock.ToList())
					.AddSetter((c, n) => c.Editable = n.EmployeeCardItem != null)
				.AddColumn("Размер")
					.AddComboRenderer(x => x.WearSize).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => SizeService.GetSize(ViewModel.UowOfDialog, x.Nomenclature.Type.SizeType,true, true))
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.SizeType != null)
				.AddColumn("Рост")
					.AddComboRenderer(x => x.Height).SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(x => SizeService.GetSize(ViewModel.UowOfDialog, x.Nomenclature.Type.HeightType, true, true))
					.AddSetter((c, n) => c.Editable = n.Nomenclature?.Type?.HeightType != null)
				.AddColumn("Процент износа")
					.AddTextRenderer(e => (e.WearPercent).ToString("P0"))
				.AddColumn("Количество")
					.AddNumericRenderer(e => e.Amount)
					.Editing(new Adjustment(0, 0, 100000, 1, 10, 1))
					.AddTextRenderer(e => e.Nomenclature != null && e.Nomenclature.Type != null && 
					                      e.Nomenclature.Type.Units != null ? e.Nomenclature.Type.Units.Name : null)
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = GetRowColor(n))
				.Finish();

			var titleFont = new Pango.FontDescription();
			titleFont.Size = (int)(14 * Pango.Scale.PangoScale);
			foreach(var column in treeItems.Columns) {

				var aLabel = new Label();
				aLabel.Markup = column.Title;
				aLabel.UseMarkup = true;
				aLabel.ModifyFont(fontDesc);
				aLabel.Show();
				column.Widget = aLabel;
				column.Alignment = 1;
			}
		}

		private string GetRowColor(ExpenseItem item)
		{
			var requiredIssue = item.EmployeeCardItem?.CalculateRequiredIssue(ViewModel.BaseParameters);
			if(requiredIssue > 0 && item.Nomenclature == null)
				return item.Amount == 0 ? "red" : "Dark red";
			if(requiredIssue > 0 && item.Amount == 0)
				return "blue";
			if(requiredIssue <= 0 && item.Amount == 0)
				return "gray";
			return null;
		}
		#endregion
		#region Обработка событий
		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.CleanEmployee();
		}
		#endregion
	}
}
