using System;
using System.Linq;
using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Workwear.Domain.Supply;

namespace Workwear.ViewModels.Supply {
	public class ShipmentDiffCauseViewModel: DialogViewModelBase, IWindowDialogSettings {
		private ShipmentItem[] selectedItems;
		
		public ShipmentDiffCauseViewModel(
			INavigationManager navigation,
			ShipmentItem[] selectedItems
		) : base(navigation) {
			this.selectedItems = selectedItems ?? throw new ArgumentNullException(nameof(selectedItems));
			DiffCause = this.selectedItems.Select(x => x.DiffCause)
				.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));;
			Title = "Заполнение причины расхождения";
		}

		
		#region Свойства View
		private string diffCause;
		public virtual string DiffCause {
			get => diffCause;
			set => SetField(ref diffCause, value);
		}
		#endregion
		
		#region Действия View
		public void FillDiffCause() {
			foreach(var item in selectedItems) {
				item.DiffCause = this.DiffCause;
			}
			Close(false, CloseSource.Save);
		}

		#endregion
		#region IWindowDialogSettings implementation
		public bool IsModal { get; } = true;
		public bool EnableMinimizeMaximize { get; } = false;
		public bool Resizable { get; } = true;
		public bool Deletable { get; } = true;
		public WindowGravity WindowPosition { get; } = WindowGravity.Center;
		#endregion
	}
}
