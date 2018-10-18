using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using workwear.Domain.Stock;

namespace workwear.JournalFilters
{
	[System.ComponentModel.ToolboxItem(true)]
	[OrmDefaultIsFiltered(true)]
	public partial class StockDocumentsFilter : RepresentationFilterBase<StockDocumentsFilter>
	{
		public StockDocumentsFilter(IUnitOfWork uow) : this()
		{
			UoW = uow;
		}

		public StockDocumentsFilter()
		{
			this.Build();
			enumcomboDocumentType.ItemsEnum = typeof(StokDocumentType);
			dateperiodDocs.StartDate = DateTime.Today.AddMonths(-1);
			dateperiodDocs.EndDate = DateTime.Today.AddDays(1);
		}

		public StokDocumentType? RestrictDocumentType
		{
			get { return enumcomboDocumentType.SelectedItem as StokDocumentType?; }
			set
			{
				enumcomboDocumentType.SelectedItem = value;
				enumcomboDocumentType.Sensitive = false;
			}
		}

		public DateTime? RestrictStartDate
		{
			get { return dateperiodDocs.StartDateOrNull; }
			set
			{
				dateperiodDocs.StartDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		public DateTime? RestrictEndDate
		{
			get { return dateperiodDocs.EndDateOrNull; }
			set
			{
				dateperiodDocs.EndDateOrNull = value;
				dateperiodDocs.Sensitive = false;
			}
		}

		protected void OnEnumcomboDocumentTypeChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}

		protected void OnDateperiodDocsPeriodChanged(object sender, EventArgs e)
		{
			OnRefiltered();
		}
	}
}
