using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using QS.Views.Dialog;
using QSOrmProject;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.OverNorms;
using Workwear.Tools.OverNorms.Impl;
using Workwear.ViewModels.Stock;
using IdToStringConverter = Gamma.Binding.Converters.IdToStringConverter;

namespace Workwear.Views.Stock 
{
	public partial class OverNormView : EntityDialogViewBase<OverNormViewModel, OverNorm> 
	{
		private const string SubstituteColumn = "SubstituteColumn";
		
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

			enumTypesComboBox.ItemsEnum = typeof(OverNormType);
			enumTypesComboBox.Binding.AddBinding(ViewModel, vm => vm.DocType, w => w.SelectedItem).InitializeFromSource();
			enumTypesComboBox.EnumItemSelected += (sender, args) => 
			{
				buttonAddEmployee.Visible = !ViewModel.OverNormModel.RequiresEmployeeIssueOperation;
				buttonAddEmployeeIssue.Visible = ViewModel.OverNormModel.RequiresEmployeeIssueOperation;

				ytreeItems.ColumnsConfig.GetColumnsByTag(SubstituteColumn)
					.First().Visible = ViewModel.OverNormModel.RequiresEmployeeIssueOperation;
			};
			
			entityWarehouseExpense.ViewModel = ViewModel.EntryWarehouseViewModel;
			
			buttonAddEmployee.Visible = !ViewModel.OverNormModel.RequiresEmployeeIssueOperation;
			buttonAddEmployeeIssue.Visible = ViewModel.OverNormModel.RequiresEmployeeIssueOperation;
			
			buttonDel.Clicked += (sender, args) => ViewModel.DeleteItem(ytreeItems.GetSelectedObject<OverNormItem>());
			buttonAddEmployee.Clicked += (sender, args) => ViewModel.SelectEmployee();
			buttonAddEmployeeIssue.Clicked += (sender, args) => ViewModel.SelectEmployeeIssue();
			buttonAddNomenclature.Clicked += (sender, args) =>
				ViewModel.SelectNomenclature(ytreeItems.GetSelectedObject<OverNormItem>());
			
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
					.AddReadOnlyTextRenderer(x => x.OverNormOperation.EmployeeIssueOperation?.Nomenclature?.Name ?? x.OverNormOperation.EmployeeIssueOperation?.ProtectionTools?.Name)
				.AddColumn("Выдаваемая номеклатура")
					.Resizable()
					.AddReadOnlyTextRenderer(x => x.OverNormOperation.WarehouseOperation?.Nomenclature.Name)
				.AddColumn("Размер").MinWidth(60)
					.AddReadOnlyTextRenderer(x => x.OverNormOperation.WarehouseOperation?.WearSize?.Name)
				.AddColumn("Рост").MinWidth(70)
					.AddReadOnlyTextRenderer(x => x.OverNormOperation.WarehouseOperation?.Height?.Name)
				.AddColumn("Количество").MinWidth(60)
					.AddReadOnlyTextRenderer(x => x.OverNormOperation.WarehouseOperation?.Amount.ToString())
				.Finish();
			
			ytreeItems.ItemsDataSource = ViewModel.Entity.Items;
			ytreeItems.Selection.Changed += (sender, args) =>
				buttonAddNomenclature.Sensitive = buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}
	}
}
