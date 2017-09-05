using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using workwear.Domain;

namespace workwear
{
	public partial class OnIssueStatement : Gtk.Bin, IParametersWidget
	{
		public string Title => "Ведомость на выдачу";

		public OnIssueStatement ()
		{
			this.Build ();

			var uow = UnitOfWorkFactory.CreateWithoutRoot ();
			//Заполняем года
			DateTime startDate = uow.Session.QueryOver<EmployeeCardItem> ()
				.Select (Projections.Min<EmployeeCardItem> (e => e.NextIssue)).SingleOrDefault<DateTime> ();
			DateTime endDate = uow.Session.QueryOver<EmployeeCardItem> ()
				.Select (Projections.Max<EmployeeCardItem> (e => e.NextIssue)).SingleOrDefault<DateTime> ();

			var years = new List<int> ();
			DateTime temp = startDate;
			do {
				years.Add (temp.Year);
				temp = temp.AddYears (1);
			} while(temp.Year <= endDate.Year);
			ylistcomboYear.ItemsList = years;
			ylistcomboYear.SelectedItem = DateTime.Today.Year;
            ylistcomboYear1.ItemsList = years;
            ylistcomboYear1.SelectedItem = DateTime.Today.Year;
            ylistcomboYear2.ItemsList = years;
            ylistcomboYear2.SelectedItem = DateTime.Today.Year;

			// Заполняем месяца
			ylistcomboMonth.SetRenderTextFunc<int> (DateWorks.GetMonthName);
            ylistcomboMonth1.SetRenderTextFunc<int>(DateWorks.GetMonthName);
            ylistcomboMonth2.SetRenderTextFunc<int>(DateWorks.GetMonthName);
			var months = new List<int> ();
			for (int i = 1; i <= 12; i++)
				months.Add (i);
			ylistcomboMonth.ItemsList = months;
			ylistcomboMonth.SelectedItem = DateTime.Today.Month;
            ylistcomboMonth1.ItemsList = months;
            ylistcomboMonth1.SelectedItem = DateTime.Today.Month;
            ylistcomboMonth2.ItemsList = months;
            ylistcomboMonth2.SelectedItem = DateTime.Today.Month;

            entryreferenceFacility.SubjectType = typeof(Facility);
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		private ReportInfo GetReportInfo()
		{
			DateTime startDate, endDate;
			if (radioPeriodMonth.Active)
			{
				startDate = new DateTime(int.Parse(ylistcomboYear.ActiveText), ylistcomboMonth.Active + 1, 1);
				endDate = new DateTime(int.Parse(ylistcomboYear.ActiveText), ylistcomboMonth.Active + 2, 1).AddDays(-1);
			}
			else
			{
				startDate = new DateTime(int.Parse(ylistcomboYear1.ActiveText), ylistcomboMonth1.Active + 1, 1);
				endDate = new DateTime(int.Parse(ylistcomboYear2.ActiveText), ylistcomboMonth2.Active + 2, 1).AddDays(-1);
			}

			return new ReportInfo
			{
				Identifier = "MonthIssueSheet",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", startDate.ToString("O").Substring(0, 10) },
					{ "end_date", endDate.ToString("O").Substring(0, 10) },
					{ "facility", entryreferenceFacility.GetSubject<Facility>()?.Id ?? -1 },
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
	}
}

