using System;
using QS.Cloud.Postomat.Client;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using Workwear.Domain.Postomats;

namespace Workwear.ViewModels.Postomats {
	public class PostomatDocumentViewModel : EntityDialogViewModelBase<PostomatDocument> {
		private readonly PostomatManagerService postomatService;

		public PostomatDocumentViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			PostomatManagerService postomatService,
			IValidator validator = null, UnitOfWorkProvider unitOfWorkProvider = null) : base(uowBuilder, unitOfWorkFactory, navigation, validator, unitOfWorkProvider) {
			this.postomatService = postomatService ?? throw new ArgumentNullException(nameof(postomatService));
		}
	}
}
