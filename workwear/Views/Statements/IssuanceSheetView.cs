using System;
using System.Linq;
using Gtk;
using QS.Views.Dialog;
using workwear.Domain.Statements;
using workwear.ViewModels.Statements;

namespace workwear.Views.Statements
{
	public partial class IssuanceSheetView : EntityDialogViewBase<IssuanceSheetViewModel, IssuanceSheet>
	{
		public IssuanceSheetView(IssuanceSheetViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg()
		{
			dateOfPreparation.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			entityentryOrganization.ViewModel = ViewModel.OrganizationEntryViewModel;
			entityentrySubdivision.ViewModel = ViewModel.SubdivisionEntryViewModel;
			entityentryResponsiblePerson.ViewModel = ViewModel.ResponsiblePersonEntryViewModel;
			entityentryHeadOfDivisionPerson.ViewModel = ViewModel.HeadOfDivisionPersonEntryViewModel;

			ytreeviewItems.CreateFluentColumnsConfig<IssuanceSheetItem>()
				.AddColumn("Ф.И.О.").Tag("IsFIOColumn").AddTextRenderer(x => x.Employee != null ? x.Employee.ShortName : String.Empty)
				.AddColumn("Спецодежда").Tag("IsNomenclatureColumn").AddTextRenderer(x => x.Nomenclature != null ? x.Nomenclature.Name : String.Empty)
				.AddColumn("Размер").AddTextRenderer(x => x.Nomenclature != null ? x.Nomenclature.Size : String.Empty)
				.AddColumn("Количество")
					.AddNumericRenderer(x => x.Amount).Editing(new Adjustment(1, 0, 100000, 1, 10, 10)).WidthChars(8)
					.AddTextRenderer(x => x.Nomenclature != null && x.Nomenclature.Type.Units != null ? x.Nomenclature.Type.Units.Name : String.Empty)
				.AddColumn("Начало эксплуатации").AddTextRenderer(x => x.StartOfUse != default(DateTime) ? x.StartOfUse.ToShortDateString() : String.Empty)
				.AddColumn("Срок службы")
					.AddNumericRenderer(x => x.Lifetime).Editing(new Adjustment(1, 0, 1000, 1, 12, 10))
				.Finish();

			ytreeviewItems.Selection.Changed += Selection_Changed;
			ytreeviewItems.Selection.Mode = SelectionMode.Multiple;
			ytreeviewItems.ItemsDataSource = ViewModel.Entity.ObservableItems;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddItems();
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			buttonDel.Sensitive = buttonSetEmployee.Sensitive = ytreeviewItems.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonDelClicked(object sender, EventArgs e)
		{
			var items = ytreeviewItems.GetSelectedObjects<IssuanceSheetItem>();
			ViewModel.RemoveItems(items);
		}

		protected void OnButtonSetEmployeeClicked(object sender, EventArgs e)
		{
			var items = ytreeviewItems.GetSelectedObjects<IssuanceSheetItem>();
			ViewModel.SetEmployee(items);
		}

		protected void OnYtreeviewItemsRowActivated(object o, RowActivatedArgs args)
		{
			if(ytreeviewItems.ColumnsConfig.GetColumnsByTag("IsFIOColumn").First() == args.Column) {
				buttonSetEmployee.Click();
			}

			if(ytreeviewItems.ColumnsConfig.GetColumnsByTag("IsNomenclatureColumn").First() == args.Column) {
				ViewModel.SetNomenclature(ytreeviewItems.GetSelectedObjects<IssuanceSheetItem>());
			}
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			ViewModel.Print();
		}
	}
}
