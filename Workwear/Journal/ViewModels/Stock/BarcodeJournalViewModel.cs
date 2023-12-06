using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Data;
using Mono.Unix.Native;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Report;
using QS.Report.ViewModels;
using QS.Services;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
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
			VisibleCreateAction = false;
			
			TableSelectionMode = JournalSelectionMode.Multiple;

			CreateFunctionPrintBarcodes();
		}

		protected override IQueryOver<Barcode> ItemsQuery(IUnitOfWork uow) 
		{
			BarcodeJournalNode resultAlias = null;
			
			Nomenclature nomenclatureAlias = null;
			Size sizeAlias = null;
			Size heightAlias = null;
			
			return uow.Session.QueryOver<Barcode>()
				.Left.JoinAlias(x => x.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(x => x.Size, () => sizeAlias)
				.Left.JoinAlias(x => x.Height, () => heightAlias)
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Title).WithAlias(() => resultAlias.Value)
					.Select(x => x.CreateDate).WithAlias(() => resultAlias.CreateDate)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Nomenclature)
					.Select(() => sizeAlias.Name).WithAlias(() => resultAlias.Size)
					.Select(() => heightAlias.Name).WithAlias(() => resultAlias.Height)
				).OrderBy(x => x.Title).Asc
				.TransformUsing(Transformers.AliasToBean<BarcodeJournalNode>());
		}
		
		#region Actions

		public void CreateFunctionPrintBarcodes() {

			NodeActionsList.Add(new JournalAction("Печать",
				(nodes) => nodes.Cast<BarcodeJournalNode>().Any(),
				(arg) => true,
				PrintBarcodes));
		}

		#endregion

		public void PrintBarcodes(object[] nodes) {
			
			var reportInfo = new ReportInfo {
				Title = "Штрихкод",
				Identifier = "Barcodes.BarcodeFromEmployeeIssue",
				Parameters = new Dictionary<string, object> {
					{"barcodes", nodes.Cast<BarcodeJournalNode>().Select(x => x.Id).ToList()}
				}
			};
			
			NavigationManager.OpenViewModel<RdlViewerViewModel, ReportInfo>(null, reportInfo);
		}
	}

	public class BarcodeJournalNode 
	{
		public int Id { get; set; }
		public string Value { get; set; }
		public string Nomenclature { get; set; }
		public string Size { get; set; }
		public string Height { get; set; }
		public DateTime CreateDate { get; set; }
	}
}
