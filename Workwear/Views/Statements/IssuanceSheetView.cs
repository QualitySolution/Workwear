﻿using System;
using System.Linq;
using Autofac;
using Gtk;
using QS.Views.Dialog;
using QS.Views.Resolve;
using workwear.Domain.Statements;
using workwear.ViewModels.Statements;

namespace workwear.Views.Statements
{
	public partial class IssuanceSheetView : EntityDialogViewBase<IssuanceSheetViewModel, IssuanceSheet>
	{
		Widget fillWidget;

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

			hboxExpense.Visible = ViewModel.VisibleExpense;
			buttonFillBy.Visible = ViewModel.VisibleFillBy;
			labelExpense.LabelProp = Entity.Expense?.Title;
			ytreeviewItems.Sensitive = ViewModel.CanEditItems;
			buttonAdd.Sensitive = ViewModel.CanEditItems;
			buttonCloseFillBy.Binding.AddBinding(ViewModel, v => v.VisibleCloseFillBy, w => w.Visible).InitializeFromSource();

			ytreeviewItems.CreateFluentColumnsConfig<IssuanceSheetItem>()
				.AddColumn("Ф.И.О.").Tag("IsFIOColumn").AddTextRenderer(x => x.Employee != null ? x.Employee.ShortName : String.Empty)
				.AddColumn("Спецодежда").Tag("IsNomenclatureColumn").AddTextRenderer(x => x.ItemName)
				.AddColumn("Размер").AddTextRenderer(x => x.Nomenclature != null ? x.Nomenclature.Size : String.Empty)
				.AddColumn("Количество")
					.AddNumericRenderer(x => x.Amount).Editing(ViewModel.CanEditItems).Adjustment(new Adjustment(1, 0, 100000, 1, 10, 10)).WidthChars(8)
					.AddTextRenderer(x => x.Nomenclature != null && x.Nomenclature.Type.Units != null ? x.Nomenclature.Type.Units.Name : String.Empty)
				.AddColumn("Начало эксплуатации").AddTextRenderer(x => x.StartOfUse != default(DateTime) ? x.StartOfUse.ToShortDateString() : String.Empty)
				.AddColumn("Срок службы")
					.AddNumericRenderer(x => x.Lifetime).Editing(ViewModel.CanEditItems).Adjustment(new Adjustment(1, 0, 1000, 1, 12, 10))
				.Finish();

			ytreeviewItems.Selection.Changed += Selection_Changed;
			ytreeviewItems.Selection.Mode = SelectionMode.Multiple;
			ytreeviewItems.ItemsDataSource = ViewModel.Entity.ObservableItems;

			Entity.PropertyChanged += Entity_PropertyChanged;
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddItems();
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			buttonDel.Sensitive = buttonSetEmployee.Sensitive = ytreeviewItems.Selection.CountSelectedRows() > 0 && ViewModel.CanEditItems;
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
			if(!ViewModel.CanEditItems)
				return;

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

		protected void OnButtonExpenseOpenClicked(object sender, EventArgs e)
		{
			ViewModel.OpenExpense();
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.ObservableItems))
				ytreeviewItems.ItemsDataSource = Entity.ObservableItems;
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName) {
				case nameof(ViewModel.FillByViewModel):
					if(fillWidget != null) {
						hboxTable.Remove(fillWidget);
						fillWidget.Destroy();
						fillWidget = null;
					}
					if(ViewModel.FillByViewModel != null) {
						var resolver = ViewModel.AutofacScope.Resolve<IGtkViewResolver>();
						fillWidget = resolver.Resolve(ViewModel.FillByViewModel);
						hboxTable.PackEnd(fillWidget, false, true, 1);
						fillWidget.Show();
					}
					break;
			}
		}

		protected void OnButtonFillByExpenseClicked(object sender, EventArgs e)
		{
			ViewModel.OpenFillBy();
		}

		protected void OnButtonCloseFillByClicked(object sender, EventArgs e)
		{
			ViewModel.CloseFillBy();
		}
	}
}
