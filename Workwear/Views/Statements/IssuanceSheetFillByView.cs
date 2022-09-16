using System;
using QS.Views;
using Workwear.Domain.Company;
using Workwear.ViewModels.Statements;

namespace workwear.Views.Statements
{

	public partial class IssuanceSheetFillByView : ViewBase<IssuanceSheetFillByViewModel>
	{
		public IssuanceSheetFillByView(IssuanceSheetFillByViewModel viewModel) : base(viewModel)
		{
			this.Build();

			daterangeIssuance.Binding.AddSource(ViewModel)
				.AddBinding(e => e.BeginDate, w => w.StartDateOrNull)
				.AddBinding(e => e.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ytreeviewEmployees.CreateFluentColumnsConfig<EmployeeCard>()
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Title)
				.AddColumn("Должность").AddTextRenderer(x => x.Post != null ? x.Post.Name : String.Empty)
				.Finish();

			ytreeviewEmployees.Selection.Changed += Selection_Changed;
			ytreeviewEmployees.SetItemsSource<EmployeeCard>(viewModel.ObservableEmployees);

			buttonFill.Binding.AddBinding(viewModel, v => v.SensetiveFillButton, w => w.Sensitive).InitializeFromSource();
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			buttonRemove.Sensitive = ytreeviewEmployees.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonFillClicked(object sender, EventArgs e)
		{
			ViewModel.FillIssuanceSheet();
		}

		protected void OnButtonAddBySubdivisionClicked(object sender, EventArgs e)
		{
			ViewModel.AddEmployeesFromDivision();
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			ViewModel.AddEmployees();
		}

		protected void OnButtonRemoveClicked(object sender, EventArgs e)
		{
			ViewModel.RemoveEmployees(ytreeviewEmployees.GetSelectedObjects<EmployeeCard>());
		}

		protected void OnRadioByExpenseToggled(object sender, EventArgs e)
		{
			if(radioByExpense.Active)
				ViewModel.Mode = IssuanceSheetFillByMode.ByExpense;
		}

		protected void OnRadioByNeedToggled(object sender, EventArgs e)
		{
			if(radioByNeed.Active)
				ViewModel.Mode = IssuanceSheetFillByMode.ByNeed;
		}
	}
}
