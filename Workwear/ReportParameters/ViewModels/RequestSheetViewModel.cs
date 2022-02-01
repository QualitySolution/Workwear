using System;
using System.Collections;
using System.Collections.Generic;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report.ViewModels;
using QS.ViewModels.Control.EEVM;
using workwear.Domain.Company;
using workwear.Domain.Stock;

namespace workwear.ReportParameters.ViewModels
{
	public class RequestSheetViewModel : ReportParametersViewModelBase, IDisposable
	{
		public RequestSheetViewModel(RdlViewerViewModel rdlViewerViewModel, IUnitOfWorkFactory uowFactory, INavigationManager navigation, ILifetimeScope AutofacScope) : base(rdlViewerViewModel)
		{
			Title = "Заявка на спецодежду";
			Identifier = "RequestSheet";

			uow = uowFactory.CreateWithoutRoot();
			var builder = new CommonEEVMBuilderFactory(rdlViewerViewModel, uow, navigation, AutofacScope);

			EntrySubdivisionViewModel = builder.ForEntity<Subdivision>().MakeByType().Finish();
		}

		private readonly IUnitOfWork uow;

		#region Entry
		public readonly EntityEntryViewModel<Subdivision> EntrySubdivisionViewModel;
		#endregion

		#region Свойства View
		private PeriodType periodType = PeriodType.Year;
		public virtual PeriodType PeriodType {
			get => periodType;
			set {
				if(SetField(ref periodType, value)) {
					switch(PeriodType) {
						case PeriodType.Year:
							PeriodLabel = "Год:";
							var listy = new List<Year>();
							var year = new Year(DateTime.Today);
							for(int i = 0; i < 4; i++) {
								listy.Add(year);
								year = year.GetNext();
							}
							PeriodList = listy;
							SelectedPeriod = listy[1];
							break;

						case PeriodType.Quarter:
							PeriodLabel = "Квартал:";
							var list = new List<Quarter>();
							var quarter = new Quarter((DateTime.Today.Month + 2) / 3, DateTime.Today.Year);
							for(int i = 0; i < 16; i++) {
								list.Add(quarter);
								quarter = quarter.GetNext();
							}
							PeriodList = list;
							SelectedPeriod = list[1];
							break;

						case PeriodType.Month:
							PeriodLabel = "Месяц:";
							var listM = new List<Month>();
							var month = new Month(DateTime.Today.AddMonths(-1));
							for(int i = 0; i < 24; i++) {
								listM.Add(month);
								month = month.GetNext();
							}
							PeriodList = listM;
							SelectedPeriod = listM[2];
							break;
						default:
							throw new NotImplementedException();
					}
				}
			}
		}

		private string periodLabel;
		public virtual string PeriodLabel {
			get => periodLabel;
			set => SetField(ref periodLabel, value);
		}

		private IList periodList;
		public virtual IList PeriodList {
			get => periodList;
			set => SetField(ref periodList, value);
		}

		private object selectedPeriod;
		public virtual object SelectedPeriod {
			get => selectedPeriod;
			set => SetField(ref selectedPeriod, value);
		}

		private IssueType? issueTypeOptions;
		public IssueType? IssueTypeOptions {
			get => issueTypeOptions;
			set => SetField(ref issueTypeOptions, value);
		}

		#endregion

		public IMonthAndYear Period => SelectedPeriod as IMonthAndYear;

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object> {
					{"begin_month", Period?.BeginMonth },
					{"begin_year", Period?.BeginYear},
					{"end_month", Period?.EndMonth},
					{"end_year", Period?.EndYear},
					{"subdivision", EntrySubdivisionViewModel.Entity?.Id ?? -1 },
					{"issue_type", IssueTypeOptions?.ToString() },
				 };

		public void Dispose()
		{
			uow.Dispose();
		}
	}

	public enum PeriodType
	{
		Month,
		Quarter,
		Year
	}

	public class Quarter : IMonthAndYear
	{
		public int Number;
		public int Year;

		public string Title => String.Format("{0} квартал {1}", Number, Year);

		public int BeginMonth => EndMonth - 2;
		public int BeginYear => Year;
		public int EndMonth => Number * 3;
		public int EndYear => Year;

		public Quarter(int num, int year)
		{
			Number = num;
			Year = year;
		}

		public Quarter GetNext()
		{
			int newNum = Number + 1;
			return newNum == 5 ? new Quarter(1, Year + 1) : new Quarter(newNum, Year);
		}
	}

	public class Month : IMonthAndYear
	{
		public int Number;
		public int Year;

		public string Title => String.Format("{0:MMMM yyyy}", new DateTime(Year, Number, 1));

		public int BeginMonth => Number;
		public int BeginYear => Year;
		public int EndMonth => Number;
		public int EndYear => Year;

		public Month(int num, int year)
		{
			Number = num;
			Year = year;
		}

		public Month(DateTime date)
		{
			Number = date.Month;
			Year = date.Year;
		}

		public Month GetNext()
		{
			int newNum = Number + 1;
			return newNum == 13 ? new Month(1, Year + 1) : new Month(newNum, Year);
		}
	}

	public class Year : IMonthAndYear
	{
		public int Number;

		public string Title => Number.ToString();

		public int BeginMonth => 1;
		public int BeginYear => Number;
		public int EndMonth => 12;
		public int EndYear => Number;

		public Year(int year)
		{
			Number = year;
		}

		public Year(DateTime date)
		{
			Number = date.Year;
		}

		public Year GetNext()
		{
			return new Year(Number + 1);
		}
	}

	public interface IMonthAndYear
	{
		int BeginMonth { get; }
		int BeginYear { get; }
		int EndMonth { get; }
		int EndYear { get; }
	}
}
