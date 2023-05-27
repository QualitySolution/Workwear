using System;
using System.Collections.Generic;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;

namespace Workwear.ViewModels.Regulations.NormChildren {
	public class NormEmployeesViewModel : ViewModelBase {
		private readonly NormViewModel parent;

		public NormEmployeesViewModel(NormViewModel parent) {
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
		}

		public IList<EmployeeNode> Employees { get; private set; }

		#region События
		public void OnShow() {
			if (Employees == null)
				UpdateNodes();
		}
		#endregion
		
		#region Действия View
		public void Add() {

		}

		public void Remove(EmployeeNode[] employee) {

		}
		#endregion

		#region private
		void UpdateNodes() {
			EmployeeNode resultAlias = null;

			Post postAlias = null;
			Subdivision subdivisionAlias = null;
			EmployeeCard employeeAlias = null;
			Norm normAlias = null;
			Department departmentAlias = null;
			
			var employees = parent.UoW.Session.QueryOver<EmployeeCard>(() => employeeAlias);
			employees.JoinAlias(x => x.UsedNorms, () => normAlias)
				.Where(x => normAlias.Id == parent.Entity.Id);
			
			Employees = employees.JoinAlias(() => employeeAlias.Post, () => postAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeAlias.Subdivision, () => subdivisionAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeAlias.Department, () => departmentAlias, JoinType.LeftOuterJoin)
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.CardNumber).WithAlias(() => resultAlias.CardNumber)
					.Select(x => x.PersonnelNumber).WithAlias(() => resultAlias.PersonnelNumber)
					.Select(x => x.FirstName).WithAlias(() => resultAlias.FirstName)
					.Select(x => x.LastName).WithAlias(() => resultAlias.LastName)
					.Select(x => x.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => employeeAlias.DismissDate).WithAlias(() => resultAlias.DismissDate)
					.Select(() => postAlias.Name).WithAlias(() => resultAlias.Post)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					.Select(() => departmentAlias.Name).WithAlias(() => resultAlias.Department)
				)
				.OrderBy(() => employeeAlias.LastName).Asc
				.ThenBy(() => employeeAlias.FirstName).Asc
				.ThenBy(() => employeeAlias.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<EmployeeNode>())
				.List<EmployeeNode>();
			
			OnPropertyChanged(nameof(Employees));
		}
		#endregion
	}
	
	public class EmployeeNode
	{
		public int Id { get; set; }
		public string CardNumber { get; set; }
		
		public string CardNumberText => CardNumber ?? Id.ToString();

		public string PersonnelNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Patronymic { get; set; }
		
		public string FIO => String.Join(" ", LastName, FirstName, Patronymic);
		
		public string Post { get; set; }
		public string Subdivision { get; set; }
		public string Department { get; set; }

		public bool Dismiss { get { return DismissDate.HasValue; } }

		public DateTime? DismissDate { get; set; }
	}
}
