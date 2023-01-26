using QS.ViewModels.Dialog;
using QS.Navigation;

using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Workwear.Domain.Sizes;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Measurements;


namespace Workwear.ViewModels.Stock.Widgets {
	public class IssueWidgetViewModel : WindowDialogViewModelBase{
		public IssueWidgetViewModel(INavigationManager navigation) : base(navigation)
		{
		}
	}
}
