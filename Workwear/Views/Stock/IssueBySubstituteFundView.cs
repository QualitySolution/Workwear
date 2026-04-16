using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Views.Dialog;
using QSOrmProject;
using Workwear.Domain.Operations;
using Workwear.Domain.Stock.Documents;
using Workwear.Tools.Features;
using Workwear.ViewModels.Stock;
using IdToStringConverter = Gamma.Binding.Converters.IdToStringConverter;

namespace Workwear.Views.Stock 
{
	public partial class IssueBySubstituteFundView : EntityDialogViewBase<IssueBySubstituteFundViewModel, SubstituteFundDocuments> 
	{
		public IssueBySubstituteFundView(IssueBySubstituteFundViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			ConfigureTreeItem();
		}

		private void ConfigureDlg() 
		{
			ylabelId.Binding
				.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter())
				.InitializeFromSource ();
			
			ylabelCreatedBy.Binding
				.AddFuncBinding(Entity, e => e.CreatedbyUser != null ? e.CreatedbyUser.Name : null, w => w.LabelProp)
				.InitializeFromSource ();
			
			ydateDoc.Binding
				.AddBinding(Entity, e => e.Date, w => w.Date)
				.InitializeFromSource();
			
			ytextComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			buttonAdd.Clicked += (sender, args) => ViewModel.SelectNomenclatureFromEmployee();
			buttonDel.Clicked += (sender, args) => ViewModel.DeleteItem(ytreeItems.GetSelectedObject<SubstituteFundDocumentItem>()); 
			buttonAddSubstitution.Clicked += (sender, args) =>
				ViewModel.SelectSubtituteNomenclature(ytreeItems.GetSelectedObject<SubstituteFundDocumentItem>());
		}
		
		private void ConfigureTreeItem() 
		{
			ytreeItems.ColumnsConfig = ColumnsConfigFactory.Create<SubstituteFundDocumentItem>()
				.AddColumn("Сотрудник")
					.Resizable()
					.AddReadOnlyTextRenderer(x => x.SubstituteFundOperation.EmployeeIssueOperation.Employee.FullName)
				.AddColumn("Заменяемая номенклатура")
					.Resizable()
					.AddReadOnlyTextRenderer(x => x.SubstituteFundOperation.EmployeeIssueOperation.Nomenclature?.Name ?? x.SubstituteFundOperation.EmployeeIssueOperation.ProtectionTools?.Name)
				.AddColumn("Номенклатура из подменного фонда")
					.Resizable()
					.AddReadOnlyTextRenderer(x => x.SubstituteFundOperation.SubstituteBarcode?.Nomenclature?.Name)
				.AddColumn("Размер").MinWidth(60)
					.AddReadOnlyTextRenderer(x => x.SubstituteFundOperation.EmployeeIssueOperation.WearSize?.Name)
				.AddColumn("Рост").MinWidth(70)
					.AddReadOnlyTextRenderer(x => x.SubstituteFundOperation.EmployeeIssueOperation.Height?.Name)
				.AddColumn ("Процент износа").MinWidth(60)
					.AddNumericRenderer(x => x.SubstituteFundOperation.EmployeeIssueOperation.WearPercent, new MultiplierToPercentConverter())
					.AddTextRenderer(e => "%", expand: false)
				.Finish();
			
			ytreeItems.Binding
				.AddBinding(Entity, e => e.Items, w => w.ItemsDataSource)
				.InitializeFromSource();
			
			ytreeItems.Selection.Changed += (sender, args) => buttonAddSubstitution.Sensitive = 
				buttonDel.Sensitive = ytreeItems.Selection.CountSelectedRows() > 0;
		}
	}
}
