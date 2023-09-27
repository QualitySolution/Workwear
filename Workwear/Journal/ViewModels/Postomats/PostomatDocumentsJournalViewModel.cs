using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using QS.Cloud.Postomat.Client;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Postomats;
using workwear.ViewModels.Postomat;

namespace workwear.Journal.ViewModels.Postomats {
	public class PostomatDocumentsJournalViewModel : EntityJournalViewModelBase<PostomatDocument, PostomatDocumentViewModel, PostomatDocumentJournalNode>
	{
		private readonly PostomatManagerService postomatManagerService;

		public PostomatDocumentsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			PostomatManagerService postomatManagerService,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) {
			this.postomatManagerService = postomatManagerService ?? throw new ArgumentNullException(nameof(postomatManagerService));

			var terminals = this.postomatManagerService.GetPostomatList();
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
			return uow.Session.QueryOver<PostomatDocument>()
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.TerminalId).WithAlias(() => resultAlias.TerminalId)
					.Select(x => x.Status).WithAlias(() => resultAlias.Status)
					.Select(x => x.Type).WithAlias(() => resultAlias.Type)
					.Select(x => x.CreateTime).WithAlias(() => resultAlias.CreateTime)
				)
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
