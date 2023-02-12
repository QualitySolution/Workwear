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

		public Action<Dictionary<int,IssueWidgetItem>> AddItems;
		
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
	
	public IssueWidgetItem(ProtectionTools protectionTools, int numberOfNeeds = 1, int numberOfIssused = 1,bool active = true) {
		this.active = active;
		this.ProtectionTools = protectionTools ?? throw new ArgumentNullException(nameof(protectionTools));
		this.type = protectionTools.Type.IssueType;
		this.numberOfNeeds = numberOfNeeds;
		this.numberOfIssused = numberOfIssused;
	}
		
	private bool active;
	public bool Active {
		get => active;
		set => SetField(ref active, value);
	}

	public ProtectionTools ProtectionTools { get; }

	private IssueType type;
	public IssueType Type {
		get => type;
	}

	private int numberOfNeeds;
	public int NumberOfNeeds {get; set; }
	
	private int numberOfIssused;
	public int NumberOfIssused {get; set; }
}
