using System;
using QS.Views.Dialog;
using Workwear.Domain.Postomats;
using Workwear.ViewModels.Postomats;

namespace Workwear.Views.Postomats {

	public partial class PostomatDocumentView : EntityDialogViewBase<PostomatDocumentViewModel, PostomatDocument> {

		public PostomatDocumentView(PostomatDocumentViewModel viewModel) : base(viewModel) {
			this.Build();
			ConfigureDlg();
			CommonButtonSubscription();
		}

		private void ConfigureDlg() {

		}
	}
}
