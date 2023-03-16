using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.ViewModels.Dialog;
using QS.Navigation;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;

namespace Workwear.ViewModels.Stock.Widgets {
	public class IssueWidgetViewModel : WindowDialogViewModelBase {
		public IssueWidgetViewModel(INavigationManager navigation, Dictionary<int,IssueWidgetItem> wigetItems) : base(navigation) {
			this.Items = wigetItems;
			Title = "Добавить для выбранных сотрудников:";
		}

		public Dictionary<int,IssueWidgetItem> Items {get; set; }

		private bool excludeOnVacation = true;
		public virtual bool ExcludeOnVacation {
			get => excludeOnVacation;
			set => SetField(ref excludeOnVacation, value);
		}
	
		public Action<Dictionary<int,IssueWidgetItem>,bool> AddItems;
		
		public void SelectAll() {
			foreach(var item in Items) 
				if(!item.Value.Active)
					item.Value.Active = true;
		}

		public void UnSelectAll() {
			foreach(var item in Items) 
				if(item.Value.Active)
					item.Value.Active = false;
		}
	};
}

public class IssueWidgetItem : PropertyChangedBase {
	
	public IssueWidgetItem(ProtectionTools protectionTools, bool active = true, int numberOfCurrentNeeds = 0, int numberOfNeeds = 1, int itemQuantityForIssuse = 0, int itemStockBalance = 0) {
		this.active = active;
		this.ProtectionTools = protectionTools ?? throw new ArgumentNullException(nameof(protectionTools));
		this.Type = protectionTools.Type.IssueType;
		this.NumberOfNeeds = numberOfNeeds;
		this.ItemQuantityForIssuse = itemQuantityForIssuse;
		this.NumberOfCurrentNeeds = numberOfCurrentNeeds;
		this.ItemStockBalance = itemStockBalance;
	}
		
	private bool active;
	public bool Active {
		get => active;
		set => SetField(ref active, value);
	}

	public ProtectionTools ProtectionTools { get; }

	public IssueType Type { get; }

	public int NumberOfNeeds {get; set; }
	public int NumberOfCurrentNeeds {get; set; }
	public int ItemQuantityForIssuse {get; set; }
	public int ItemStockBalance {get; set; }
}
