using System;
using QS.ViewModels.Dialog;
using QS.Navigation;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using QS.DomainModel.Entity;
using System.Collections.Generic;

namespace Workwear.ViewModels.Stock.Widgets {
	public class IssueWidgetViewModel : WindowDialogViewModelBase {
		public IssueWidgetViewModel(INavigationManager navigation, Dictionary<int,IssueWidgetItem> wigetItems) : base(navigation) {
			this.items = wigetItems;
			Title = "Добавить для выбранных сотрудников:";
		}

		private Dictionary<int,IssueWidgetItem> items;
		public Dictionary<int,IssueWidgetItem> Items {
			get => items;
			set => items = value;
		}
		
		public class IssueWigetArgs : EventArgs {
			public readonly Dictionary<int,IssueWidgetItem> wigetItems;
			public IssueWigetArgs(Dictionary<int,IssueWidgetItem> wigetItems) {
				this.wigetItems = wigetItems;
			}
		}

		public Action AddItems;
		
		public void SelectAll() {
			foreach(var item in items) 
				if(!item.Value.Active)
					item.Value.Active = true;
		}

		public void UnSelectAll() {
			foreach(var item in items) 
				if(item.Value.Active)
					item.Value.Active = false;
		}
	};
}

public class IssueWidgetItem : PropertyChangedBase {
	
	public IssueWidgetItem(ProtectionTools protectionTools, int numberOfNeeds = 1, int numberOfIssused = 1,bool active = true) {
		this.active = active;
		this.protectionTools = protectionTools ?? throw new ArgumentNullException(nameof(protectionTools));
		this.type = protectionTools.Type.IssueType;
		this.numberOfNeeds = numberOfNeeds;
		this.numberOfIssused = numberOfIssused;
	}
		
	private bool active;
	public bool Active {
		get => active;
		set => SetField(ref active, value);
	}

	private ProtectionTools protectionTools;
	public ProtectionTools ProtectionTools {
		get => protectionTools;
	}

	private IssueType type;
	public IssueType Type {
		get => type;
	}

	private int numberOfNeeds;
	public int NumberOfNeeds {
		get => numberOfNeeds;
		set => numberOfNeeds = value;
	}
	
	private int numberOfIssused;
	public int NumberOfIssused {
		get => numberOfIssused;
		set => numberOfIssused = value;
	}
}
