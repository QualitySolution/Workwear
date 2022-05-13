using System;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace workwear.ViewModels.Stock.Widgets
{
	public class BuhDocumentViewModel : WindowDialogViewModelBase
	{
		public delegate void SetChangeAndClose(object sender, BuhDocEventArgs args);

		public event SetChangeAndClose SetChangeAndCloseEvent;
		public BuhDocumentViewModel(INavigationManager navigation, string buhDocText) : base(navigation)
		{
			BuhDocText = buhDocText;
			Title = "Введите бухгалтерский документ";
		}
		
		private string buhDocText;
		public string BuhDocText {
			get => buhDocText;
			set => SetField(ref buhDocText, value);
		}
		public void AddChange() {
			Close(false, CloseSource.Self);
			SetChangeAndCloseEvent?.Invoke(this, new BuhDocEventArgs{Value = BuhDocText});
		}
		public void Cancel() => Close(false, CloseSource.Cancel);
	}
	
	public class BuhDocEventArgs : EventArgs
	{
		public string Value { get; set; }
	}
}
