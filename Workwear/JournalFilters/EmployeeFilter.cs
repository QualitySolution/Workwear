using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;

namespace workwear.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	[OrmDefaultIsFiltered(true)]
	public partial class EmployeeFilter : RepresentationFilterBase<EmployeeFilter>
	{
		public EmployeeFilter(IUnitOfWork uow)
		{
			UoW = uow;
			this.Build();
		}

		public bool RestrictOnlyWork
		{
			get { return checkCardsOnlyActual.Active; }
			set
			{
				checkCardsOnlyActual.Active = value;
				checkCardsOnlyActual.Sensitive = false;
			}
		}

		protected void OnCheckCardsOnlyActualClicked(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}
