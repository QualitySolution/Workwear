using System;
using QS.ViewModels.Dialog;
using QS.Navigation;
using Workwear.Domain.Regulations;
using Workwear.Domain.Stock;
using QS.DomainModel.Entity;
using System.Collections.Generic;

using System.Linq;
using FluentNHibernate.Data;
using QS.DomainModel.UoW;
using Workwear.Domain.Stock;
using Workwear.Domain.Stock.Documents;
using Workwear.Measurements;


namespace Workwear.ViewModels.Stock.Widgets {
	public class IssueWidgetViewModel : WindowDialogViewModelBase {
		public IssueWidgetViewModel(INavigationManager navigation, CollectiveExpenseItemsViewModel entityViewModel, Dictionary<int,IssueWidgetItem> wigetItems) : base(navigation) {
			this.items = wigetItems;
			this.entityViewModel = entityViewModel;
		}

		private Dictionary<int,IssueWidgetItem> items;
		public Dictionary<int,IssueWidgetItem> Items {
			get => items;
			set => items = value;
		}

		private CollectiveExpenseItemsViewModel entityViewModel;

//Пока не понял как вклинить местные arg в событие gtk надо разобраться, сейчас пробрасываю ссылку на справочник		
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
	
	public IssueWidgetItem(ProtectionTools protectionTools, Nomenclature nomenclature, int numberOfNeeds = 1, bool active = true) {
		this.active = active;
		this.protectionTools = protectionTools ?? throw new ArgumentNullException(nameof(protectionTools));
		this.nomenclature = nomenclature;// ?? throw new ArgumentNullException(nameof(nomenclature)");
		this.type = protectionTools.Type.IssueType;
		this.numberOfNeeds = numberOfNeeds;
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

	private Nomenclature nomenclature;
	public Nomenclature Nomenclature {
		get => nomenclature;
		set => nomenclature = value;
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
}
