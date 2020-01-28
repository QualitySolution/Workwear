using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using QS.ViewModels;
using workwear.Domain.Company;

namespace workwear.ViewModels.Statements
{
	public class IssuanceSheetFillByExpenseViewModel : ViewModelBase
	{
		#region Notify

		private DateTime beginDate;
		public virtual DateTime BeginDate {
			get => beginDate;
			set => SetField(ref beginDate, value);
		}

		private DateTime endDate;
		public virtual DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}

		#endregion

		List<EmployeeCard> employees = new List<EmployeeCard>();

		GenericObservableList<EmployeeCard> observableEmployees;

		public GenericObservableList<EmployeeCard> ObservableEmployees { get {
				if(observableEmployees == null)
					observableEmployees = new GenericObservableList<EmployeeCard>(employees);

				return observableEmployees;
			}
		}
	}
}
