using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using workwear.Domain;
using workwear.JournalFilters;

namespace workwear.ViewModel
{
	public class EmployeesVM : RepresentationModelEntityBase<EmployeeCard, EmployeesVMNode>
	{
		public EmployeeFilter Filter
		{
			get
			{
				return RepresentationFilter as EmployeeFilter;
			}
			set
			{
				RepresentationFilter = (IRepresentationFilter)value;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			EmployeesVMNode resultAlias = null;

			Post postAlias = null;
			Facility facilityAlias = null;
			EmployeeCard employeeAlias = null;

			var employees = UoW.Session.QueryOver<EmployeeCard> (() => employeeAlias);
			if (Filter.RestrictOnlyWork)
				employees.Where(x => x.DismissDate == null);

			var employeesList = employees
				.JoinAlias (() => employeeAlias.Post, () => postAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias (() => employeeAlias.Facility, () => facilityAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList (list => list
					.Select (() => employeeAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => employeeAlias.CardNumber).WithAlias (() => resultAlias.CardNumber)
					.Select (() => employeeAlias.PersonnelNumber).WithAlias (() => resultAlias.PersonnelNumber)
					.Select (() => employeeAlias.FirstName).WithAlias (() => resultAlias.FirstName)
					.Select (() => employeeAlias.LastName).WithAlias (() => resultAlias.LastName)
					.Select (() => employeeAlias.Patronymic).WithAlias (() => resultAlias.Patronymic)
					.Select (() => employeeAlias.DismissDate).WithAlias (() => resultAlias.DismissDate)
					.Select (() => postAlias.Name).WithAlias (() => resultAlias.Post)
					.Select (() => facilityAlias.Name).WithAlias (() => resultAlias.Subdivision)
				)
				.TransformUsing (Transformers.AliasToBean<EmployeesVMNode> ())
				.List<EmployeesVMNode> ();

			SetItemsSource (employeesList.ToList ());
		}

		IColumnsConfig treeViewConfig = ColumnsConfigFactory.Create<EmployeesVMNode>()
			.AddColumn ("Номер").AddTextRenderer (node => node.CardNumber)
			.AddColumn ("Табельный №").AddTextRenderer (node => node.PersonnelNumber)
			.AddColumn ("Ф.И.О.").AddTextRenderer (node => node.FIO)
			.AddColumn ("Должность").AddTextRenderer (node => node.Post)
			.AddColumn ("Объект").AddTextRenderer (node => node.Subdivision)
			.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Foreground = x.Dismiss ? "gray" : "black")
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return treeViewConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelEntityBase

		protected override bool NeedUpdateFunc (EmployeeCard updatedSubject)
		{
			return true;
		}

		#endregion

		public EmployeesVM () : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new EmployeeFilter(UoW);
		}

		public EmployeesVM (IUnitOfWork uow) : base ()
		{
			this.UoW = uow;
		}
	}

	public class EmployeesVMNode
	{
		public int Id { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string CardNumber { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string PersonnelNumber { get; set; }

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Patronymic { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string FIO { get{
				return String.Join(" ", LastName, FirstName, Patronymic);
			} }

		[UseForSearch]
		[SearchHighlight]
		public string Post { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Subdivision { get; set; }

		public bool Dismiss { get{return DismissDate.HasValue;} }

		public DateTime? DismissDate { get; set; }
	}
}

