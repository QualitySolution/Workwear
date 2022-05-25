using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Utilities;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Company;

namespace workwear.Representations.Organization
{
	public class EmployeeVacationsVM : RepresentationModelEntityBase<EmployeeVacation, EmployeeVacationsVMNode>
	{
		EmployeeCard employee;

		public EmployeeCard Employee {
			get => employee;
			set => employee = value;
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			if(Employee == null) {
				SetItemsSource(new List<EmployeeVacationsVMNode>());
				return;
			}

			EmployeeVacationsVMNode resultAlias = null;
			EmployeeVacation vacationAlias = null;
			VacationType vacationTypeAlias = null;

			var query = UoW.Session.QueryOver<EmployeeVacation>(() => vacationAlias)
				.Where(e => e.Employee == Employee);

			var vacationList = query
				.JoinAlias(() => vacationAlias.VacationType, () => vacationTypeAlias)
				.SelectList(list => list
				   .Select(() => vacationAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => vacationTypeAlias.Name).WithAlias(() => resultAlias.VacationTypeName)
				   .Select(() => vacationAlias.BeginDate).WithAlias(() => resultAlias.BeginDate)
				   .Select(() => vacationAlias.EndDate).WithAlias(() => resultAlias.EndDate)
				   .Select(() => vacationAlias.Comments).WithAlias(() => resultAlias.Comments)
				)
				.OrderBy(() => vacationAlias.EndDate).Desc
				.TransformUsing(Transformers.AliasToBean<EmployeeVacationsVMNode>())
				.List<EmployeeVacationsVMNode>();

			SetItemsSource(vacationList);
		}

		protected override bool NeedUpdateFunc(EmployeeVacation updatedSubject)
		{
			return updatedSubject.Employee.Id == Employee.Id;
		}

		readonly IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<EmployeeVacationsVMNode>()
			.AddColumn("Вид отпуска").AddTextRenderer(e => e.VacationTypeName)
			.AddColumn("С даты").AddTextRenderer(e => e.BeginDateText)
			.AddColumn("По дату").AddTextRenderer(e => e.EndDateText)
			.AddColumn("Длительность").AddTextRenderer(e => e.VacationTime)
			.AddColumn("Комментарий").AddTextRenderer(e => e.Comments)
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return treeViewConfig; }
		}

		#endregion

		public EmployeeVacationsVM(IUnitOfWork uow)
		{
			this.UoW = uow;
		}
}

	public class EmployeeVacationsVMNode
	{
		public int Id { get; set; }

		public string VacationTypeName { get; set; }

		public DateTime BeginDate { get; set; }

		public DateTime EndDate { get; set; }

		public string Comments { get; set; }

		public string BeginDateText => BeginDate.ToShortDateString();

		public string EndDateText => EndDate.ToShortDateString();

		public string VacationTime => NumberToTextRus.FormatCase(
			(EndDate - BeginDate).Days + 1,
			"{0} день",
			"{0} дня",
			"{0} дней");
	}
}
