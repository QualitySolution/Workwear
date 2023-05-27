using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.Navigation;
using QS.ViewModels;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Company;

namespace Workwear.ViewModels.Regulations.NormChildren {
	public class NormEmployeesViewModel : ViewModelBase {
		private readonly NormViewModel parent;
		private readonly INavigationManager navigation;
		private readonly IInteractiveQuestion interactive;
		private readonly ModalProgressCreator progressCreator;

		public NormEmployeesViewModel(NormViewModel parent, INavigationManager navigation, IInteractiveQuestion interactive, ModalProgressCreator progressCreator) {
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			this.progressCreator = progressCreator ?? throw new ArgumentNullException(nameof(progressCreator));
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
			if(parent.UoW.HasChanges) {
				if(!interactive.Question("Для добавления нормы сотрудникам необходимо сохранить изменения в норме. Сохранить?") || !parent.Save())
					return;
			}

			var selectJournal = navigation.OpenViewModel<EmployeeJournalViewModel>(parent, OpenPageOptions.AsSlave);
			selectJournal.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Multiple;
			selectJournal.ViewModel.OnSelectResult += LoadEmployees;
		}
		
		void LoadEmployees(object sender, QS.Project.Journal.JournalSelectedEventArgs e) {
			var selectedIds = e.GetSelectedObjects<EmployeeJournalNode>().Select(x => x.Id).ToArray();
			progressCreator.Start(selectedIds.Length + 2, text: "Загрузка сотрудников");
			var newEmployees = parent.UoW.GetById<EmployeeCard>(selectedIds);
			foreach(var employee in newEmployees) {
				progressCreator.Add(text: $"Добавление нормы для {employee.ShortName}");
				employee.AddUsedNorm(parent.Entity);
			}
			progressCreator.Add(text: "Сохранение изменений");
			parent.UoW.Commit();
			progressCreator.Add(text: "Обновление списка сотрудников");
			UpdateNodes();
			progressCreator.Close();
		}

		public void Remove(EmployeeNode[] employees) {
			if(parent.UoW.HasChanges) {
				if(!interactive.Question("Для удаления нормы у сотрудников необходимо сохранить изменения нормы. Сохранить?") || !parent.Save())
					return;
			}

			progressCreator.Start(employees.Length + 2, text: "Загрузка сотрудников");
			var newEmployees = parent.UoW.GetById<EmployeeCard>(employees.Select(x => x.Id).ToArray());
			foreach(var employee in newEmployees) {
				progressCreator.Add(text: $"Удаление нормы у {employee.ShortName}");
				employee.RemoveUsedNorm(parent.Entity);
			}

			progressCreator.Add(text: "Сохранение изменений");
			parent.UoW.Commit();
			progressCreator.Add(text: "Обновление списка сотрудников");
			UpdateNodes();
			progressCreator.Close(); 
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
