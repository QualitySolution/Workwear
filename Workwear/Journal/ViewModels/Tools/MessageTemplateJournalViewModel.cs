using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using workwear.Domain.Tools;
using workwear.ViewModels.Tools;

namespace workwear.Journal.ViewModels.Tools
{
	public class MessageTemplateJournalViewModel: EntityJournalViewModelBase<MessageTemplate, MessageTemplateViewModel, NotificationTemplateJournalNode>
	{
		public MessageTemplateJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigationManager, IDeleteEntityService deleteEntityService = null, ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			Title = "Шаблоны уведомлений";
			UseSlider = true;
		}

		protected override IQueryOver<MessageTemplate> ItemsQuery(IUnitOfWork uow)
		{
			NotificationTemplateJournalNode resultAlias = null;

			return uow.Session.QueryOver<MessageTemplate>()
					.Where(GetSearchCriterion<MessageTemplate>(
					x => x.Id,
					x => x.Name,
					x => x.MessageTitle,
					x => x.MessageText
 					))
				.SelectList((list) => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.MessageTitle).WithAlias(() => resultAlias.MessageTitle)
					.Select(x => x.MessageText).WithAlias(() => resultAlias.MessageText)
				).OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NotificationTemplateJournalNode>());
		}
	}
	public class NotificationTemplateJournalNode
	{
		public int Id { get; set; }
		public string Name { get; internal set; }
		public string MessageTitle { get; internal set; }
		public string MessageText { get; internal set; }
	}
}
