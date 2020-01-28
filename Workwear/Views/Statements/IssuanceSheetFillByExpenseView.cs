using System;
using QS.Views;
using workwear.Domain.Company;
using workwear.ViewModels.Statements;

namespace workwear.Views.Statements
{

	public partial class IssuanceSheetFillByExpenseView : ViewBase<IssuanceSheetFillByExpenseViewModel>
	{
		public IssuanceSheetFillByExpenseView(IssuanceSheetFillByExpenseViewModel viewModel) : base(viewModel)
		{
			this.Build();

			daterangeIssuance.Binding.AddSource(ViewModel)
				.AddBinding(e => e.BeginDate, w => w.StartDate)
				.AddBinding(e => e.EndDate, w => w.EndDate)
				.InitializeFromSource();

			ytreeviewEmployees.CreateFluentColumnsConfig<EmployeeCard>()
				.AddColumn("Сотрудник").AddTextRenderer(x => x.Title)
				.AddColumn("Должность").AddTextRenderer(x => x.Post != null ? x.Post.Name : String.Empty)
				.Finish();
				
			ytreeviewEmployees.SetItemsSource<EmployeeCard>(viewModel.ObservableEmployees);
		}
	}
}
