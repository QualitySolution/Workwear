using System;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace workwear.ViewModels.Stock
{
	public class IssueByIdentifierViewModel : WindowDialogViewModelBase
	{
		public IssueByIdentifierViewModel(IUnitOfWorkFactory unitOfWorkFactory, INavigationManager navigation) : base(navigation)
		{
			IsModal = false;
		}

		private string cardId;
		public virtual string CardID {
			get => cardId;
			set => SetField(ref cardId, value);
		}

	}
}
