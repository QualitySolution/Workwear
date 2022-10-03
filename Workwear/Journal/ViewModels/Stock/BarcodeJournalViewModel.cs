using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Stock.Barcodes;
using Workwear.ViewModels.Stock;

namespace workwear.Journal.ViewModels.Stock 
{
	public class BarcodeJournalViewModel : EntityJournalViewModelBase<Barcode, BarcodeViewModel, BarcodeJournalNode>
	{
		public BarcodeJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			IInteractiveService interactiveService, 
			INavigationManager navigationManager, 
			IDeleteEntityService deleteEntityService = null, 
			ICurrentPermissionService currentPermissionService = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) 
		{
			UseSlider = true;
		}

		protected override IQueryOver<Barcode> ItemsQuery(IUnitOfWork uow) 
		{
			BarcodeJournalNode resultAlias = null;
			return uow.Session.QueryOver<Barcode>()
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Value).WithAlias(() => resultAlias.Value)
				).OrderBy(x => x.Value).Asc
				.TransformUsing(Transformers.AliasToBean<BarcodeJournalNode>());
		}
	}

	public class BarcodeJournalNode 
	{
		public int Id { get; set; }
		public string Value { get; set; }
	}
}
