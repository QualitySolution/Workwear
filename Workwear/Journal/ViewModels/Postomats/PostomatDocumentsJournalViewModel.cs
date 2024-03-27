using System;
using System.Collections.Generic;
using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Postomats;
using Workwear.Journal.Filter.ViewModels.Postomats;
using Workwear.ViewModels.Postomats;

namespace workwear.Journal.ViewModels.Postomats {
	public class PostomatDocumentsJournalViewModel : EntityJournalViewModelBase<PostomatDocument, PostomatDocumentViewModel, PostomatDocumentJournalNode>
	{
		private readonly PostomatManagerService postomatManagerService;
		public PostomatDocumentsJournalFilterViewModel Filter { get; set; }

		public PostomatDocumentsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			PostomatManagerService postomatManagerService,
			ILifetimeScope autofacScope,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) {
			this.postomatManagerService = postomatManagerService ?? throw new ArgumentNullException(nameof(postomatManagerService));

			JournalFilter = Filter = autofacScope.Resolve<PostomatDocumentsJournalFilterViewModel>(new TypedParameter(typeof(JournalViewModelBase), this));

			VisibleDeleteAction = false;
			
			var terminals = this.postomatManagerService.GetPostomatList(PostomatListType.All);
			foreach (var terminal in terminals) {
				Terminals.Add(terminal.Id, (terminal.Name, terminal.Location));
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
