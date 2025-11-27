using System;
using Autofac;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;
using workwear.Journal.ViewModels.Regulations;
using Workwear.ViewModels.Company;
using Workwear.ViewModels.Regulations;

namespace workwear.Journal.Filter.ViewModels.Company
{
	public class EmployeeFilterViewModel : JournalFilterViewModelBase<EmployeeFilterViewModel>
	{
		#region Ограничения

		private bool showOnlyWork = true;
		public virtual bool ShowOnlyWork {
			get => showOnlyWork;
			set => SetField(ref showOnlyWork, value);
		}
		
		private bool canShowOnlyWithoutNorms = false;
		public bool CanShowOnlyWithoutNorms {
			get => canShowOnlyWithoutNorms;
			set => SetField(ref canShowOnlyWithoutNorms, value);
		}
		private bool showOnlyWithoutNorms = false;
		public virtual bool ShowOnlyWithoutNorms {
			get => showOnlyWithoutNorms;
			set => SetField(ref showOnlyWithoutNorms, value);
		}

		private bool excludeInVacation = false;
		public virtual bool ExcludeInVacation {
			get => excludeInVacation;
			set => SetField(ref excludeInVacation, value);
		}
		
		private Subdivision subdivision;
		public virtual Subdivision Subdivision {
			get => subdivision;
			set {
				SetField(ref subdivision, value);
				if(Department != null && Department.Subdivision != Subdivision)
					Department = null;
			}
		}

		private Department department;
		public virtual Department Department {
			get => department;
			set {
				if(SetField(ref department, value))
					if(department != null && !DomainHelper.EqualDomainObjects(Subdivision, department?.Subdivision))
						Subdivision = department?.Subdivision;
			}
		}
		
		private Post post;
		public virtual Post Post {
			get => post;
			set {
				SetField(ref post, value);
				if(value != null) {
					Department = value.Department;
					Subdivision = value.Subdivision;
				}
			}
		}
		
		private Norm norm;
		public virtual Norm Norm {
			get => norm;
			set => SetField(ref norm, value);
		}
		
		private DateTime date = DateTime.Today;
		public virtual DateTime Date {
			get => date;
			set => SetField(ref date, value);
		}
		
		#endregion

		#region EntityModels

		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
		public EntityEntryViewModel<Department> DepartmentEntry;
		public EntityEntryViewModel<Post> PostEntry;
		public EntityEntryViewModel<Norm> NormEntry;
		#endregion

		public EmployeeFilterViewModel(JournalViewModelBase journal, INavigationManager navigation, ILifetimeScope autofacScope, IUnitOfWorkFactory unitOfWorkFactory = null) : base(journal, unitOfWorkFactory)
		{
			var builder = new CommonEEVMBuilderFactory<EmployeeFilterViewModel>(journal, this, UoW, navigation, autofacScope);

			SubdivisionEntry = builder.ForProperty(x => x.Subdivision)
				.MakeByType()
				.Finish();

			DepartmentEntry = builder.ForProperty(x => x.Department)
				.MakeByType()
				.Finish();
			DepartmentEntry.EntitySelector = new DepartmentJournalViewModelSelector(journal, navigation, SubdivisionEntry);
			
			PostEntry = builder.ForProperty(x => x.Post)
				.MakeByType()
				.Finish();
			///TODO Хорошо бы реализовать автопроставку фильтра подразделения и отдела в журнале должжностей
			
			NormEntry = builder.ForProperty(x => x.Norm)				
				.MakeByType()				
				.Finish();
			
			NormEntry = builder.ForProperty(x => x.Norm)
				.UseViewModelJournalAndAutocompleter<NormJournalViewModel>()
				.UseViewModelDialog<NormViewModel>()
				.Finish();
		}
	}
}
