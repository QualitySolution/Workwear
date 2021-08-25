﻿using System.Collections.Generic;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Company;
using workwear.Domain.Regulations;
using workwear.Journal.Filter.ViewModels.Regulations;
using workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations
{
	public class NormJournalViewModel : EntityJournalViewModelBase<Norm, NormViewModel, NormJournalNode>
	{
		public NormFilterViewModel Filter { get; private set; }

		public NormJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, ILifetimeScope autofacScope, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
			AutofacScope = autofacScope;
			JournalFilter = Filter = AutofacScope.Resolve<NormFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));
			CreatePopupActions();
		}

		protected override IQueryOver<Norm> ItemsQuery(IUnitOfWork uow)
		{
			NormJournalNode resultAlias = null;

			Post postAlias = null;
			Subdivision subdivisionAlias = null;
			Norm normAlias = null;
			RegulationDoc regulationDocAlias = null;
			RegulationDocAnnex docAnnexAlias = null;

			var norms = uow.Session.QueryOver<Norm>(() => normAlias)
				.JoinAlias(n => n.Document, () => regulationDocAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(n => n.Annex, () => docAnnexAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => normAlias.Posts, () => postAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => postAlias.Subdivision, () => subdivisionAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(GetSearchCriterion(
					() => normAlias.Name,
					() => postAlias.Name,
					() => subdivisionAlias.Name
					));
			if(Filter.Post != null)
				norms.Where(x => x.Id == Filter.Post.Id);

			return norms
				.SelectList(list => list
				   .SelectGroup(() => normAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => regulationDocAlias.Number).WithAlias(() => resultAlias.TonNumber)
				   .Select(() => docAnnexAlias.Number).WithAlias(() => resultAlias.AnnexNumber)
				   .Select(() => normAlias.TONParagraph).WithAlias(() => resultAlias.TonParagraph)
				   .Select(() => normAlias.Name).WithAlias(() => resultAlias.Name)
				   .Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( CONCAT_WS(' ', ?1, CONCAT('[', ?2 ,']')) SEPARATOR ?3)"),
					   NHibernateUtil.String,
					   Projections.Property(() => postAlias.Name),
					   Projections.Property(() => subdivisionAlias.Name),
					   Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.Posts)
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

		public string Posts { get; set; }
	}
}
