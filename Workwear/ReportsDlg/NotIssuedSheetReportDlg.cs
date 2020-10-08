using System;
using System.Collections.Generic;
using Autofac;
using QS.Dialog.Gtk;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QSReport;
using workwear.Domain.Company;
using workwear.Journal.ViewModels.Company;
using workwear.ViewModels.Company;

namespace workwear
{
	public partial class NotIssuedSheetReportDlg : Gtk.Bin, IParametersWidget
	{
		ILifetimeScope AutofacScope;
		IUnitOfWork uow;

		public NotIssuedSheetReportDlg ()
		{
			this.Build ();
			ydateReport.Date = DateTime.Today;

			uow = UnitOfWorkFactory.CreateWithoutRoot();
			AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
			Func<ITdiTab> getTab = () => DialogHelper.FindParentTab(this);
			var builder = new LegacyEEVMBuilderFactory(getTab, uow, MainClass.MainWin.NavigationManager, AutofacScope);

			entitySubdivision.ViewModel = builder.ForEntity<Subdivision>()
				.UseViewModelJournalAndAutocompleter<SubdivisionJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();
		}

		string IParametersWidget.Title => "Справка по невыданному";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			if (LoadReport != null)
			{
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), true));
			}
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
				{
					{ "report_date", ydateReport.Date },
					{ "only_missing", checkOnlyMissing.Active },
					{"subdivision_id", entitySubdivision.ViewModel.Entity == null ? -1 : DomainHelper.GetId(entitySubdivision.ViewModel.Entity)}
				};

			return new ReportInfo
			{
				Identifier = "NotIssuedSheet",
				Parameters = parameters
			};
		}

		protected void OnYdateReportDateChanged(object sender, EventArgs e)
		{
			buttonRun.Sensitive = !ydateReport.IsEmpty;
		}

		public override void Destroy()
		{
			uow.Dispose();
			AutofacScope.Dispose();
			base.Destroy();
		}
	}
}