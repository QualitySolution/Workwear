using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Journal;
using QS.Project.Services;
using QS.ViewModels.Extension;
using Workwear.Domain.Postomats;
using Workwear.Journal.Filter.ViewModels.Postomats;
using Workwear.Tools;
using Workwear.ViewModels.Postomats;

namespace workwear.Journal.ViewModels.Postomats {
	public class PostomatDocumentsJournalViewModel : EntityJournalViewModelBase<PostomatDocument, PostomatDocumentViewModel, PostomatDocumentJournalNode>, IDialogDocumentation
	{
		private readonly PostomatManagerService postomatManagerService;
		private readonly IInteractiveQuestion interactive;
		public PostomatDocumentsJournalFilterViewModel Filter { get; set; }
		#region IDialogDocumentation
		public string DocumentationUrl => DocHelper.GetDocUrl("postomat.html#postamat-refill-journal");
		public string ButtonTooltip => DocHelper.GetJournalDocTooltip(typeof(PostomatDocument));
		#endregion
		public PostomatDocumentsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			PostomatManagerService postomatManagerService,
			ILifetimeScope autofacScope,
			IInteractiveQuestion interactive,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) {
			this.postomatManagerService = postomatManagerService ?? throw new ArgumentNullException(nameof(postomatManagerService));
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));

			JournalFilter = Filter = autofacScope.Resolve<PostomatDocumentsJournalFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			VisibleDeleteAction = false;
			
			var terminals = this.postomatManagerService.GetPostomatList(PostomatListType.Aso);
			foreach (var terminal in terminals) {
				Terminals.Add(terminal.Id, (terminal.Name, terminal.Location));
			}

			#region Добавляем действия

			NodeActionsList.Add(new JournalAction(
				"Отменить документ",
				selected => selected.OfType<PostomatDocumentJournalNode>().Any(n => n.Status == DocumentStatus.New),
				selected => true,
				selected => DisableDoc(selected.OfType<PostomatDocumentJournalNode>().ToList())
			));

			#endregion
		}

		void DisableDoc(List<PostomatDocumentJournalNode> rows) {
			if(!interactive.Question("Вы уверены что хотите отменить выбранные документы? Вернуть статус документа будет не возможно."))
				return;
			var ids = rows.Where(x => x.Status == DocumentStatus.New)
				.Select(x => x.Id).ToArray();
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Отмена документов постамата из журнала")) {
				uow.GetAll<PostomatDocument>()
					.Where(doc => ids.Contains(doc.Id))
					.UpdateBuilder()
					.Set(doc => doc.Status, DocumentStatus.Deleted)
					.Update();
			}
		}

		#region Терминалы
		readonly IDictionary<uint, (string Name, string Location)> Terminals = new Dictionary<uint, (string Name, string Location)>();
		public string GetTerminalName(uint id) => Terminals.ContainsKey(id) ? Terminals[id].Name : null;
		public string GetTerminalLocation(uint id) => Terminals.ContainsKey(id) ? Terminals[id].Location : null;
		#endregion
		
		
		
		protected override IQueryOver<PostomatDocument> ItemsQuery(IUnitOfWork uow) {
			PostomatDocumentJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<PostomatDocument>();

			if(!Filter.ShowClosed)
				query.Where(x => x.Status == DocumentStatus.New);
			return query
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.TerminalId).WithAlias(() => resultAlias.TerminalId)
					.Select(x => x.Status).WithAlias(() => resultAlias.Status)
					.Select(x => x.Type).WithAlias(() => resultAlias.Type)
					.Select(x => x.CreateTime).WithAlias(() => resultAlias.CreateTime)
				).OrderBy(x => x.Id).Desc
				.TransformUsing(Transformers.AliasToBean<PostomatDocumentJournalNode>());
		}
	}
	
	public class PostomatDocumentJournalNode
	{
		public int Id { get; set; }
		public uint TerminalId { get; set; }
		public DocumentStatus Status { get; set; }
		public DocumentType Type { get; set; }
		public DateTime CreateTime { get; set; }
	}
}
