using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Workwear.Domain.Operations;

namespace Workwear.Domain.Stock
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "штрих-коды",
		Nominative = "штрих-код",
		Genitive = "штрих-кода"
		)]
	[HistoryTrace]
	public class Barcode : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; }

		private string title;
		[Display (Name = "Значение")]
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}

		private string fractional;
		[Display (Name = "Часть от всей выдачи")]
		public virtual string Fractional {
			get => fractional;
			set => SetField(ref fractional, value);
		}

		private EmployeeIssueOperation employeeIssueOperation;
		[Display (Name = "Привязаная операция выдачи сотруднику")]
		public virtual EmployeeIssueOperation EmployeeIssueOperation {
			get => employeeIssueOperation;
			set => SetField(ref employeeIssueOperation, value);
		}
	}
}
