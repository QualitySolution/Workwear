using System;
using QSOrmProject;
using workwear.Domain;
using NHibernate.Criterion;
using System.Collections.Generic;
using QSProjectsLib;

namespace workwear
{
	public partial class OnIssueStatement : Gtk.Dialog
	{
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

			// Заполняем месяца
			ylistcomboMonth.SetRenderTextFunc<int> (DateWorks.GetMonthName);
			var months = new List<int> ();
			for (int i = 1; i <= 12; i++)
				months.Add (i);
			ylistcomboMonth.ItemsList = months;
			ylistcomboMonth.SelectedItem = DateTime.Today.Month;
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			string param = String.Format("month={0}&year={1}", ylistcomboMonth.SelectedItem, ylistcomboYear.SelectedItem) ;
			ViewReportExt.Run("MonthIssueSheet", param);
		}
	}
}

