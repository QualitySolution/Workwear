using System;
using System.Collections.Generic;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal;

namespace Workwear.Journal.Filter.ViewModels.ClothingService {
	public class ClaimsJournalFilterViewModel : JournalFilterViewModelBase<ClaimsJournalFilterViewModel> {
		
		public ClaimsJournalFilterViewModel(
			JournalViewModelBase journalViewModel,
			PostomatManagerService postomatService,
			IUnitOfWorkFactory unitOfWorkFactory = null)
			: base(journalViewModel, unitOfWorkFactory)
		{
			if(postomatService == null) throw new ArgumentNullException(nameof(postomatService));
			var postomats = new List<PostomatInfo>();
			postomat = new PostomatInfo() { Id = 0, Name = "Любой" };//заодно проставим умолчания
			postomats.Add(postomat);
			postomats.AddRange(postomatService.GetPostomatList(PostomatListType.Aso));
			Postomats = postomats;
		}
		
		public IList<PostomatInfo> Postomats { get; }
		
		#region Ограничения
		private bool showClosed;
		public virtual bool ShowClosed {
			get => showClosed;
			set => SetField(ref showClosed, value);
		}

		private PostomatInfo postomat;
		[PropertyChangedAlso(nameof(PostomatId))]
		public PostomatInfo Postomat {
			get => postomat;
			set => SetField(ref postomat, value);
		}

		public uint PostomatId => Postomat?.Id ?? 0;
		#endregion

		#region Sensetive
		private bool sensitiveShowClosed = true;
		public virtual bool SensitiveShowClosed {
			get => sensitiveShowClosed;
			set => SetField(ref sensitiveShowClosed, value);
		}
		
		private bool sensitivePostomat = true;
		public virtual bool SensitivePostomat {
			get => sensitivePostomat;
			set => SetField(ref sensitivePostomat, value);
		}
		#endregion
	}
}
