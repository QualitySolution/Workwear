﻿using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;

namespace Workwear.Journal.Filter.ViewModels.Company {
	public class DepartmentFilterViewModel  : JournalFilterViewModelBase<DepartmentFilterViewModel>
	{
		public DepartmentFilterViewModel(JournalViewModelBase journal, 
			INavigationManager navigation, 
			ILifetimeScope autofacScope, 
			IUnitOfWorkFactory unitOfWorkFactory = null) : base(journal, unitOfWorkFactory) 
		{
			var builder = new CommonEEVMBuilderFactory<DepartmentFilterViewModel>(journal, this, UoW, navigation, autofacScope);
			EntrySubdivision = builder.ForProperty(x => x.Subdivision).MakeByType().Finish();
		}
		
		#region Ограничения
		private Subdivision subdivision;
		public Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}
		#endregion
		#region EntityModels
		public EntityEntryViewModel<Subdivision> EntrySubdivision;
		#endregion
		
	}
}
