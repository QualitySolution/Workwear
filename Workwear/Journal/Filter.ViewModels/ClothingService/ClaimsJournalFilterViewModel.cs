using System;
using System.Collections.Generic;
using QS.Cloud.Postomat.Client;
using QS.Cloud.Postomat.Manage;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using Workwear.Domain.ClothingService;
using Workwear.Tools.Features;

namespace Workwear.Journal.Filter.ViewModels.ClothingService {
	public class ClaimsJournalFilterViewModel : JournalFilterViewModelBase<ClaimsJournalFilterViewModel> {
		private readonly FeaturesService featuresService;

		public ClaimsJournalFilterViewModel(
			JournalViewModelBase journalViewModel,
			PostomatManagerService postomatService,
			FeaturesService featuresService,
			IUnitOfWorkFactory unitOfWorkFactory = null)
			: base(journalViewModel, unitOfWorkFactory)
		{
			if(postomatService == null) throw new ArgumentNullException(nameof(postomatService));
			this.featuresService = featuresService ?? throw new ArgumentNullException(nameof(featuresService));
			var postomats = new List<PostomatInfo>();
			postomat = new PostomatInfo() { Id = 0, Name = "Любой" };//заодно проставим умолчания
			postomats.Add(postomat);
			if(featuresService.Available(WorkwearFeature.Postomats))
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
		
		private bool showOnlyRepair;
		public virtual bool ShowOnlyRepair {
			get => showOnlyRepair;
			set => SetField(ref showOnlyRepair, value);
		}
		
		private ClaimState? status;
		public virtual ClaimState? Status {
			get => status;
			set => SetField(ref status, value);
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
