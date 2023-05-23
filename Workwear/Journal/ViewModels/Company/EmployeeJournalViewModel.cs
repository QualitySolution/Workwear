using System;
using Autofac;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities.Text;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Journal.Filter.ViewModels.Company;
using Workwear.Tools.Features;
using Workwear.ViewModels.Company;

namespace workwear.Journal.ViewModels.Company
{
	public class EmployeeJournalViewModel : EntityJournalViewModelBase<EmployeeCard, EmployeeViewModel, EmployeeJournalNode>
	{
		/// <summary>
		/// Для хранения пользовательской информации как в WinForms
		/// </summary>
		public object Tag;

		public readonly FeaturesService FeaturesService;
		private readonly Norm norm;

		public EmployeeFilterViewModel Filter { get; private set; }

		public EmployeeJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, 
										IDeleteEntityService deleteEntityService, ILifetimeScope autofacScope, FeaturesService featuresService, ICurrentPermissionService currentPermissionService = null,
										//Ограничения журнала
										Norm norm = null) 
										: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = false;
			
			JournalFilter = Filter = autofacScope.Resolve<EmployeeFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
			this.FeaturesService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			this.norm = norm;

			if(norm != null) {
				norm = UoW.GetById<Norm>(norm.Id); //Загружаем из своей сессии.
				Title = "Использующие норму: " + norm.Title;
			}
		}

		protected override IQueryOver<EmployeeCard> ItemsQuery(IUnitOfWork uow)
		{
			EmployeeJournalNode resultAlias = null;

			Post postAlias = null;
			Subdivision subdivisionAlias = null;
			EmployeeCard employeeAlias = null;
			Norm normAlias = null;
			Department departmentAlias = null;

			var vacationSubquery = QueryOver.Of<EmployeeVacation>()
				.Where(ev => ev.Employee.Id == employeeAlias.Id)
				.Where(ev => ev.BeginDate <= DateTime.Today && ev.EndDate >= DateTime.Today)
				.Select(ev => ev.Id)
				.Take(1);

			var employees = uow.Session.QueryOver<EmployeeCard>(() => employeeAlias);
			if(Filter.ShowOnlyWork)
				employees.Where(x => x.DismissDate == null);
			if(Filter.Subdivision != null)
				employees.Where(x => x.Subdivision.Id == Filter.Subdivision.Id);
			if(Filter.Department != null)
				employees.Where(x => x.Department.Id == Filter.Department.Id);

			if(norm != null) {
				employees.JoinAlias(x => x.UsedNorms, () => normAlias)
				 .Where(x => normAlias.Id == norm.Id);
			}

			return employees
				.Where(GetSearchCriterion(
					() => employeeAlias.Id,
					() => employeeAlias.CardNumber,
					() => employeeAlias.PersonnelNumber,
					() => employeeAlias.LastName,
					() => employeeAlias.FirstName,
					() => employeeAlias.Patronymic,
					() => postAlias.Name,
					() => subdivisionAlias.Name,
					() => departmentAlias.Name,
					() => employeeAlias.Comment
 					))

				.JoinAlias(() => employeeAlias.Post, () => postAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeAlias.Subdivision, () => subdivisionAlias, JoinType.LeftOuterJoin)
				.JoinAlias(() => employeeAlias.Department, () => departmentAlias, JoinType.LeftOuterJoin)
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.CardNumber).WithAlias(() => resultAlias.CardNumber)
					.Select(x => x.PersonnelNumber).WithAlias(() => resultAlias.PersonnelNumber)
					.Select(x => x.CardKey).WithAlias(() => resultAlias.CardKey)
					.Select(x => x.FirstName).WithAlias(() => resultAlias.FirstName)
					.Select(x => x.LastName).WithAlias(() => resultAlias.LastName)
					.Select(x => x.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(() => employeeAlias.DismissDate).WithAlias(() => resultAlias.DismissDate)
					.Select(() => postAlias.Name).WithAlias(() => resultAlias.Post)
	   				.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					.Select(x => x.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => departmentAlias.Name).WithAlias(() => resultAlias.Department)
					.SelectSubQuery(vacationSubquery).WithAlias(() => resultAlias.VacationId)
 					)
				.OrderBy(() => employeeAlias.LastName).Asc
				.ThenBy(() => employeeAlias.FirstName).Asc
				.ThenBy(() => employeeAlias.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<EmployeeJournalNode>());
		}
	}

	public class EmployeeJournalNode
	{
		public int Id { get; set; }
		[SearchHighlight]
		public string CardNumber { get; set; }

		[SearchHighlight]
		public string CardNumberText {
			get {
				return CardNumber ?? Id.ToString();
			}
		}

		[SearchHighlight]
		public string PersonnelNumber { get; set; }
		public string CardKey { get; set; }

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Patronymic { get; set; }

		[SearchHighlight]
		public string FIO {
			get {
				return String.Join(" ", LastName, FirstName, Patronymic);
			}
		}
		[SearchHighlight]
		public string Post { get; set; }
		[SearchHighlight]
		public string Subdivision { get; set; }

		public bool Dismiss { get { return DismissDate.HasValue; } }

		public DateTime? DismissDate { get; set; }

		public int? VacationId { get; private set; }

		public bool InVocation => VacationId.HasValue;

		[SearchHighlight]
		public string Comment { get; set; }

		public string Title => PersonHelper.PersonNameWithInitials(LastName, FirstName, Patronymic);
		public string Department { get; set; }
	}
}
