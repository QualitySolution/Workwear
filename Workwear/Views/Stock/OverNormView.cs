using System;
using System.ComponentModel;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock.Documents;
using Workwear.ViewModels.Stock;

namespace Workwear.Views.Stock 
{
	public partial class OverNormView : EntityDialogViewBase<OverNormViewModel, OverNorm> 
	{
		private const string SubstituteColumn = "SubstituteColumn";
		private const string BarcodesColumn = "BarcodesColumn";
		private OverNormItem selectedItem;
		
		public OverNormView(OverNormViewModel viewModel) : base(viewModel) 
		{
			this.Build();
			CommonButtonSubscription();
			ConfigureDlg();
			ConfigureTreeItem();
		}
		
		private void ConfigureDlg() 
		{
			entryId.Binding.AddSource(ViewModel)
					.AddBinding(vm => vm.DocNumber, w => w.Text)
					.AddBinding(vm => vm.SensitiveDocNumber, w => w.Sensitive)
					.InitializeFromSource();
			checkAuto.Binding.AddBinding(ViewModel, vm => vm.AutoDocNumber, w => w.Active).InitializeFromSource();
			
			ylabelCreatedBy.Binding
				.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp)
				.InitializeFromSource ();
			ydateDoc.Binding
				.AddBinding(Entity, e => e.Date, w => w.Date)
				.InitializeFromSource();
			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();
			buttonAddEmployee.Binding
				.AddBinding(ViewModel, wm => wm.CanAddItems, w => w.Sensitive)
				.InitializeFromSource();
			buttonAddEmployeeIssue.Binding
				.AddBinding(ViewModel, wm => wm.CanAddItems, w => w.Sensitive)
				.InitializeFromSource();
			buttonDel.Binding
				.AddBinding(ViewModel, wm => wm.CanRemoveActiveItem, w => w.Sensitive)
				.InitializeFromSource();
			buttonAddNomenclature.Binding
				.AddBinding(ViewModel, wm => wm.CanChoiceForActiveItem, w => w.Sensitive)
				.InitializeFromSource();
			
			enumTypesComboBox.ItemsEnum = typeof(OverNormType);
			enumTypesComboBox.AddEnumToHideList(ViewModel.HiddenOverNormTypes.ToArray());
			enumTypesComboBox.Binding.AddBinding(ViewModel, vm => vm.DocType, w => w.SelectedItem).InitializeFromSource();
			enumTypesComboBox.EnumItemSelected += (sender, args) => {
				buttonAddEmployee.Visible = !ViewModel.OverNormModel.RequiresEmployeeIssueOperation;
				buttonAddEmployeeIssue.Visible = ViewModel.OverNormModel.RequiresEmployeeIssueOperation;

				ytreeItems.ColumnsConfig.GetColumnsByTag(SubstituteColumn)
					.First().Visible = ViewModel.OverNormModel.RequiresEmployeeIssueOperation;
				
				ytreeItems.ColumnsConfig.GetColumnsByTag(BarcodesColumn)
					.First().Visible = ViewModel.OverNormModel.CanUseWithBarcodes;
				
				UpdateDelBarcodesMenu();
			};
			
			entityWarehouseExpense.ViewModel = ViewModel.EntryWarehouseViewModel;
			
			buttonAddEmployee.Visible = !ViewModel.OverNormModel.RequiresEmployeeIssueOperation;
			buttonAddEmployeeIssue.Visible = ViewModel.OverNormModel.RequiresEmployeeIssueOperation;
			
			buttonDel.Clicked += (s, a) => ViewModel.DeleteItem(ytreeItems.GetSelectedObject<OverNormItem>());
			buttonAddEmployee.Clicked += (s, a) => ViewModel.SelectEmployees();
			buttonAddEmployeeIssue.Clicked += (s, a) => ViewModel.SelectEmployeeIssue();
			buttonAddNomenclature.Clicked += (s, a) => ViewModel.SelectNomenclature(ytreeItems.GetSelectedObject<OverNormItem>());
			
			labelSum.Binding
				.AddBinding(ViewModel, vm => vm.Total, w => w.LabelProp)
				.InitializeFromSource();
		}
		
		private void ConfigureTreeItem() 
		{
			ytreeItems.ColumnsConfig = ColumnsConfigFactory.Create<OverNormItem>()
				.AddColumn("Сотрудник")
					.Resizable()
					.AddReadOnlyTextRenderer(x => x.OverNormOperation.Employee.FullName)
				.AddColumn("Заменяемая номенклатура")
					.Tag(SubstituteColumn)
					.Visible(ViewModel.OverNormModel.RequiresEmployeeIssueOperation)
					.Resizable()
					.AddReadOnlyTextRenderer(x => x.OverNormOperation.SubstitutedIssueOperation?.Nomenclature?.Name ?? x.OverNormOperation.SubstitutedIssueOperation?.ProtectionTools?.Name)
				.AddColumn("Выдаваемая номенклатура")
					.Resizable()
					.AddReadOnlyTextRenderer(x => x.OverNormOperation.WarehouseOperation?.Nomenclature.Name)
				.AddColumn("Размер").MinWidth(60)
					.AddReadOnlyTextRenderer(x => x.OverNormOperation.WarehouseOperation?.WearSize?.Name)
				.AddColumn("Рост").MinWidth(70)
					.AddReadOnlyTextRenderer(x => x.OverNormOperation.WarehouseOperation?.Height?.Name)
				.AddColumn("Количество").MinWidth(60)
					.AddNumericRenderer(x => x.Amount)
					.Editing(new Adjustment(1, 1, int.MaxValue, 1, 10, 0), ViewModel.CanEdit)
					.AddSetter((c, x) => {
						c.Editable = ViewModel.CanEdit && x.CanEditAmount;
						c.Adjustment.Upper = x.MaxAmount;
					})
			.AddColumn("Штрихкоды")
				.Tag(BarcodesColumn)
				.Visible(ViewModel.OverNormModel.CanUseWithBarcodes)
				.AddReadOnlyTextRenderer(x =>  string.Join("\n", x.Barcodes.Select(b => b.Title)))
				.Finish();
			
			ytreeItems.ItemsDataSource = Entity.Items;
			ytreeItems.Selection.Changed += ytreeItems_Selection_Changed;
		}

		private void ytreeItems_Selection_Changed(object sender, EventArgs e)
		{
			ChangeSelectedItem(ytreeItems.GetSelectedObject<OverNormItem>());
			UpdateDelBarcodesMenu();
		}

		private void ChangeSelectedItem(OverNormItem item)
		{
			if(selectedItem == item) {
				ViewModel.SelectedItem = item;
				return;
			}

			if(selectedItem != null)
				selectedItem.PropertyChanged -= SelectedItemOnPropertyChanged;

			selectedItem = item;
			ViewModel.SelectedItem = item;

			if(selectedItem != null)
				selectedItem.PropertyChanged += SelectedItemOnPropertyChanged;
		}

		private void SelectedItemOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(OverNormItem.Barcodes))
				UpdateDelBarcodesMenu();
		}
		
		private void UpdateDelBarcodesMenu() 
		{
			OverNormItem item = ytreeItems.GetSelectedObject<OverNormItem>();
			ChangeSelectedItem(item);
			buttonDelBarcodes.Menu = null;
			
			buttonDelBarcodes.Sensitive = ViewModel.CanRemoveActiveItem &&
			                              ViewModel.OverNormModel.CanUseWithBarcodes &&
			                              item != null &&
			                              item.OverNormOperation?.BarcodeOperations != null &&
			                              item.OverNormOperation.BarcodeOperations.Count > 1;
			if (buttonDelBarcodes.Sensitive && item != null) {
				Menu barcodesMenu = new Menu();
				yMenuItem menuItem;
				foreach (var bo in item.OverNormOperation.BarcodeOperations) {
					var barcode = bo.Barcode;
					menuItem = new yMenuItem(barcode.Title);
					menuItem.Activated += (o, eventArgs) => {
						ViewModel.DeleteBarcodeFromItem(item, barcode);
						UpdateDelBarcodesMenu();
						ytreeItems.QueueDraw();
					};
					
					barcodesMenu.Add(menuItem);
				}

				buttonDelBarcodes.Menu = barcodesMenu;
				barcodesMenu.ShowAll();
			}
		}
	}
}
