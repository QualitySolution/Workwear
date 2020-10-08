using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate.Criterion;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Tdi;
using QS.Utilities;
using QS.ViewModels.Control.EEVM;
using QSReport;
using workwear.Domain.Company;
using workwear.Journal.ViewModels.Company;
using workwear.ViewModels.Company;

namespace workwear
{
	public partial class OnIssueStatement : Gtk.Bin, IParametersWidget
	{
		public string Title => "Ведомость на выдачу";

		private bool errorStartDate = false;
		ILifetimeScope AutofacScope;
		IUnitOfWork uow;

		public OnIssueStatement ()
		{
			this.Build ();

			uow = UnitOfWorkFactory.CreateWithoutRoot ();

			// Заполняем месяца
			ylistcomboMonth.SetRenderTextFunc<int> (DateHelper.GetMonthName);
            ylistcomboMonth1.SetRenderTextFunc<int>(DateHelper.GetMonthName);
            ylistcomboMonth2.SetRenderTextFunc<int>(DateHelper.GetMonthName);
			var months = new List<int> ();
			for (int i = 1; i <= 12; i++)
				months.Add (i);
			ylistcomboMonth.ItemsList = months;
			ylistcomboMonth.SelectedItem = DateTime.Today.Month;
            ylistcomboMonth1.ItemsList = months;
            ylistcomboMonth1.SelectedItem = DateTime.Today.Month;
            ylistcomboMonth2.ItemsList = months;
            ylistcomboMonth2.SelectedItem = DateTime.Today.Month;

			AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();

			Func<ITdiTab> getTab = () => DialogHelper.FindParentTab(this);

			var builder = new LegacyEEVMBuilderFactory(getTab, uow, MainClass.MainWin.NavigationManager, AutofacScope);

			entityEmployee.ViewModel = builder.ForEntity<EmployeeCard>()
							.UseViewModelJournalAndAutocompleter<EmployeeJournalViewModel>()
							.UseViewModelDialog<EmployeeViewModel>()
							.Finish();

			entityEmployee.ViewModel.ChangedByUser += OnEntryEmploeeChangedByUser;

			entitySubdivision.ViewModel = builder.ForEntity<Subdivision>()
				.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();

			entitySubdivision.ViewModel.ChangedByUser += OnEntryreferenceFacilityChangedByUser;

			//Заполняем года
			DateTime startDate = uow.Session.QueryOver<EmployeeCardItem>()
				.Select(Projections.Min<EmployeeCardItem>(e => e.NextIssue)).SingleOrDefault<DateTime>();
			DateTime endDate = uow.Session.QueryOver<EmployeeCardItem>()
				.Select(Projections.Max<EmployeeCardItem>(e => e.NextIssue)).SingleOrDefault<DateTime>();

			if(startDate == default(DateTime)) {
				buttonRun.Sensitive = false;
				labelError.Markup = "<span foreground=\"red\">У всех сотрудников нет потребности в выдачи спецодежды. Сначала заполните карточки сотрудников.</span>";
				errorStartDate = true;
				return;
			}

			var years = new List<int>();
			DateTime temp = startDate;
			do {
				years.Add(temp.Year);
				temp = temp.AddYears(1);
			} while(temp.Year <= endDate.Year);
			ylistcomboYear.ItemsList = years;
			ylistcomboYear1.ItemsList = years;
			ylistcomboYear2.ItemsList = years;
			int setYear;
			if(years.Contains(DateTime.Today.Year))
				setYear = DateTime.Today.Year;
			else
				setYear = years.Max();

			ylistcomboYear.SelectedItem = setYear;
			ylistcomboYear1.SelectedItem = setYear;
			ylistcomboYear2.SelectedItem = setYear;

		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		private ReportInfo GetReportInfo()
		{
			DateTime startDate, endDate;
			if (radioPeriodMonth.Active)
			{
				startDate = new DateTime(int.Parse(ylistcomboYear.ActiveText), ylistcomboMonth.Active + 1, 1);
				endDate = new DateTime(int.Parse(ylistcomboYear.ActiveText), ylistcomboMonth.Active + 1, 1).AddMonths(1).AddDays(-1);
			}
			else
			{
				startDate = new DateTime(int.Parse(ylistcomboYear1.ActiveText), ylistcomboMonth1.Active + 1, 1);
				endDate = new DateTime(int.Parse(ylistcomboYear2.ActiveText), ylistcomboMonth2.Active + 1, 1).AddMonths(1).AddDays(-1);
			}

			return new ReportInfo
			{
				Identifier = "MonthIssueSheet",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", startDate.ToString("O").Substring(0, 10) },
					{ "end_date", endDate.ToString("O").Substring(0, 10) },
					{ "facility", entitySubdivision.ViewModel.GetEntity<Subdivision>()?.Id ?? -1 },
					{ "employee", entityEmployee.ViewModel.GetEntity<EmployeeCard>()?.Id ?? -1 },
					{"ShowSignature", true}
				}
			};
		}

        protected void OnRadioPeriodMonthToggled(object sender, EventArgs e)
        {
            UpdatePeriodWidgets();
            CheckPeriod();
        }

        private void UpdatePeriodWidgets()
        {
            ylistcomboMonth.Sensitive = ylistcomboYear.Sensitive = radioPeriodMonth.Active;
            ylistcomboMonth1.Sensitive = ylistcomboMonth2.Sensitive =
                ylistcomboYear1.Sensitive = ylistcomboYear2.Sensitive = radioPeriodMultiMonth.Active;
        }

        protected void OnRadioPeriodMultiMonthToggled(object sender, EventArgs e)
        {
            UpdatePeriodWidgets();
        }

        private void CheckPeriod()
        {
			if(errorStartDate)
				return;

            if(!radioPeriodMultiMonth.Active){
				buttonRun.Sensitive = true;
                return;
            }

            bool invalid;
            if(ylistcomboYear1.Active == ylistcomboYear2.Active)
            {
                invalid = ylistcomboMonth1.Active > ylistcomboMonth2.Active;
            }
            else{
                invalid = ylistcomboYear1.Active > ylistcomboYear2.Active;
            }
			buttonRun.Sensitive = !invalid;
        }

        protected void OnYlistcomboMonth1Changed(object sender, EventArgs e)
        {
            CheckPeriod();
        }

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), true));
			}
		}

		protected void OnEntryreferenceFacilityChangedByUser(object sender, EventArgs e)
		{
			entityEmployee.ViewModel.IsEditable = entitySubdivision.ViewModel.Entity == null;
		}

		protected void OnEntryEmploeeChangedByUser(object sender, EventArgs e)
		{
			entitySubdivision.ViewModel.IsEditable = entityEmployee.ViewModel.Entity == null;
		}

		public override void Destroy()
		{
			uow.Dispose();
			AutofacScope.Dispose();
			base.Destroy();
		}
	}
}

