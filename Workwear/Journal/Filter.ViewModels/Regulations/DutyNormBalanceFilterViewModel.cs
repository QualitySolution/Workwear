using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using Workwear.Domain.Company;
using Workwear.Domain.Regulations;

namespace Workwear.Journal.Filter.ViewModels.Regulations {
	public class DutyNormBalanceFilterViewModel: JournalFilterViewModelBase<DutyNormBalanceFilterViewModel> {

		public DutyNormBalanceFilterViewModel(
			JournalViewModelBase journalViewModel,
			INavigationManager navigation, 
			ILifetimeScope autofacScope,
			IUnitOfWorkFactory unitOfWorkFactory=null
			) : base(journalViewModel, unitOfWorkFactory)
		{
			CanNotify = false;
			var builder = new CommonEEVMBuilderFactory<DutyNormBalanceFilterViewModel>(
				journalViewModel, this, UoW, navigation, autofacScope);
			DutyNormEntry = builder.ForProperty(x => x.DutyNorm).MakeByType().Finish();
			SubdivisionEntry = builder.ForProperty(x => x.Subdivision).MakeByType().Finish();
			Date = DateTime.Today;
			DutyNorm = dutyNorm;
			CanNotify = true;

		}

		private DutyNorm dutyNorm;
		public virtual DutyNorm DutyNorm {
			get => dutyNorm;
			set {
				if(SetField(ref dutyNorm, value))
					if(value != null) {
						SubdivisionSensitive = false;
						Subdivision = dutyNorm.Subdivision;
					}
					else {
						SubdivisionSensitive = true;
						Subdivision = null;
					}
			}
		}
		
		private Subdivision subdivision;
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		private DateTime date;
		public virtual DateTime Date {
			get => date;
			set => SetField(ref date, value);
		}

		private bool dateSensitive;
		public virtual bool DateSensitive {
			get => dateSensitive;
			set => SetField(ref dateSensitive, value);
		}
		
		private bool dutyNormSensitive;
		public virtual bool DutyNormSensitive {
			get => dutyNormSensitive;
			set => SetField(ref dutyNormSensitive, value);
		}
		
		private bool subdivisionSensitive = true;
		public virtual bool SubdivisionSensitive {
			get => subdivisionSensitive;
			set => SetField(ref subdivisionSensitive, value);
		}
		
		public EntityEntryViewModel<DutyNorm> DutyNormEntry;
		public EntityEntryViewModel<Subdivision> SubdivisionEntry;
	}
}
