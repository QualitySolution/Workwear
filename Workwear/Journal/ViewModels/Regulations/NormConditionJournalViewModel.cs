using System;
using System.Globalization;
using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Regulations;
using workwear.ViewModels.Regulations;

namespace workwear.Journal.ViewModels.Regulations
{
	public class NormConditionJournalViewModel: EntityJournalViewModelBase<NormCondition, NormConditionViewModel, NormConditionJournalNode>
	{
		public NormConditionJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, ILifetimeScope autofacScope, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			UseSlider = true;
			AutofacScope = autofacScope;
			Title = "Условия нормы";
		}

		protected override IQueryOver<NormCondition> ItemsQuery(IUnitOfWork uow)
		{
			NormConditionJournalNode resultAlias = null;
			NormCondition normConditionAlias = null;

			var normsCondition = uow.Session.QueryOver(() => normConditionAlias);

			return normsCondition
				.SelectList(list => list
				   .Select(() => normConditionAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => normConditionAlias.Name).WithAlias(() => resultAlias.Name)
				   .Select(() => normConditionAlias.SexNormCondition).WithAlias(() => resultAlias.Sex)
				   .Select(() => normConditionAlias.IssuanceStart).WithAlias(() => resultAlias.DateStart)
				   .Select(() => normConditionAlias.IssuanceEnd).WithAlias(() => resultAlias.DateEnd)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NormConditionJournalNode>());
		}
	}
}
public class NormConditionJournalNode
{
	static CultureInfo culture = CultureInfo.GetCultureInfo("ru-RU");

	public int Id { get; set; }

	public string Name { get; set; }

	public SexNormCondition Sex { get; set; }

	public DateTime? DateStart { get; set; }

	public string DateStringStart {
		get => DateStart is null ? "" : DateStart.Value.ToString("m", culture); 
	}

	public string DateStringEnd {
		get => DateEnd is null ? "" : DateEnd.Value.ToString("m", culture);
	}

	public DateTime? DateEnd { get; set; }

}