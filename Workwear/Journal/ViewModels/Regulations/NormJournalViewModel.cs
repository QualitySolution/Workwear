using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations
{
	public class NormJournalViewModel : EntityJournalViewModelBase<Norm, NormViewModel, NormJournalNode>
	{
		public NormJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
			CreatePopupActions();
		}

		protected override IQueryOver<Norm> ItemsQuery(IUnitOfWork uow)
		{
			NormJournalNode resultAlias = null;

			Post professionAlias = null;
			Norm normAlias = null;
			RegulationDoc regulationDocAlias = null;
			RegulationDocAnnex docAnnexAlias = null;

			var norms = uow.Session.QueryOver<Norm>(() => normAlias);

			return norms
				.JoinAlias(n => n.Document, () => regulationDocAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(n => n.Annex, () => docAnnexAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => normAlias.Professions, () => professionAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(GetSearchCriterion(
					() => normAlias.Name,
					() => normAlias.TONParagraph
					))
				.SelectList(list => list
				   .SelectGroup(() => normAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => regulationDocAlias.Number).WithAlias(() => resultAlias.TonNumber)
				   .Select(() => docAnnexAlias.Number).WithAlias(() => resultAlias.AnnexNumber)
				   .Select(() => normAlias.TONParagraph).WithAlias(() => resultAlias.TonParagraph)
				   .Select(() => normAlias.Name).WithAlias(() => resultAlias.Name)
				   .Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
					   NHibernateUtil.String,
					   Projections.Property(() => professionAlias.Name),
					   Projections.Constant("; "))
				   ).WithAlias(() => resultAlias.Professions)
				)
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NormJournalNode>());
		}

		#region Popupmenu action implementation

		protected override List<IJournalAction> PopupActionsList { get; set; } = new List<IJournalAction>();
		/// <summary>
		/// Действия, выполняемые в popup меню.
		/// </summary>
		public override IEnumerable<IJournalAction> PopupActions => base.PopupActions; 
		protected override void CreatePopupActions()
		{
			IEnumerable<IJournalAction> popupmenuActions = new List<IJournalAction> {
				new JournalAction("Копировать норму",(arg) => true,(arg) => arg.Length == 1,CopyNorm)
			};
			PopupActionsList.AddRange(popupmenuActions);
		}
		private void CopyNorm(object[] nodes)
		{
			if(nodes.Length != 1)
				return;
			int normId = (nodes[0] as NormJournalNode).Id;
			var page = NavigationManager.OpenViewModel<NormViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForCreate());
			page.ViewModel.CopyNormFrom(normId);
		}
		#endregion
	}

	public class NormJournalNode
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string TonNumber { get; set; }

		public int? AnnexNumber { get; set; }

		public string TonAttachment => AnnexNumber?.ToString();

		public string TonParagraph { get; set; }

		public string Professions { get; set; }
	}
}
