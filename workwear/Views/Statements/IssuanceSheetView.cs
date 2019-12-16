using System;
using Gtk;
using QS.Views.GtkUI;
using workwear.Domain.Regulations;
using workwear.Domain.Statements;
using workwear.ViewModels.Statements;

namespace workwear.Views.Statements
{
	public partial class IssuanceSheetView : EntityTabViewBase<IssuanceSheetViewModel, IssuanceSheet>
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
				.AddColumn("Ф.И.О.").AddTextRenderer(x => x.Employee != null ? x.Employee.ShortName : String.Empty)
				.AddColumn("Спецодежда").AddTextRenderer(x => x.Nomenclature != null ? x.Nomenclature.Name : String.Empty)
				.AddColumn("Размер").AddTextRenderer(x => x.Nomenclature != null ? x.Nomenclature.Size : String.Empty)
				.AddColumn("Количество")
					.AddNumericRenderer(x => x.Amount).Editing(new Adjustment(1, 0, 100000, 1, 10, 10)).WidthChars(8)
					.AddTextRenderer(x => x.Nomenclature != null && x.Nomenclature.Type.Units != null ? x.Nomenclature.Type.Units.Name : String.Empty)
				.AddColumn("Начало эксплуатации").AddTextRenderer(x => x.StartOfUse.ToShortDateString())
				.AddColumn("Срок службы")
					.AddNumericRenderer(x => x.PeriodCount).Editing(new Adjustment(1, 0, 1000, 1, 12, 10))
					.AddEnumRenderer(x => x.PeriodType).Editing()
				.Finish();

			ytreeviewItems.Selection.Mode = SelectionMode.Multiple;
			ytreeviewItems.ItemsDataSource = ViewModel.Entity.ObservableItems;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddItems();
		}

		protected void OnButtonDelClicked(object sender, EventArgs e)
		{
			var items = ytreeviewItems.GetSelectedObjects<IssuanceSheetItem>();
			ViewModel.RemoveItems(items);
		}
	}
}
