using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Workwear.Domain.Postomats;
using Workwear.ViewModels.Postomats;

namespace workwear.Journal.ViewModels.Postomats {
	public class PostomatDocumentsWithdrawJournalViewModel : EntityJournalViewModelBase<PostomatDocumentWithdraw, PostomatDocumentWithdrawViewModel, PostomatDocumentWithdrawJournalNode>
	{
		private readonly PostomatManagerService postomatService;

		public PostomatDocumentsWithdrawJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			PostomatManagerService postomatService,
			ILifetimeScope autofacScope,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService) {
			this.postomatService = postomatService ?? throw new ArgumentNullException(nameof(postomatService));
			
			VisibleDeleteAction = false;
		}

	
		protected override IQueryOver<PostomatDocumentWithdraw> ItemsQuery(IUnitOfWork uow) {
			PostomatDocumentWithdrawJournalNode resultAlias = null;

			IQueryOver<PostomatDocumentWithdraw, PostomatDocumentWithdraw> query =
				uow.Session.QueryOver<PostomatDocumentWithdraw>();
			
			return query
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.CreateTime).WithAlias(() => resultAlias.CreateTime)
					.Select(x => x.User).WithAlias(() => resultAlias.User)
				)
				.OrderBy(x => x.CreateTime).Desc
				.TransformUsing(Transformers.AliasToBean<PostomatDocumentWithdrawJournalNode>());
		}
	}
	
	public class PostomatDocumentWithdrawJournalNode
	{
		public int Id { get; set; }
		public DateTime CreateTime { get; set; }
		public UserBase User { get; set; }
	}
}
