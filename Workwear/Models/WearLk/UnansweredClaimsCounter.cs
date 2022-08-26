using System;
using System.Linq;
using Gtk;

namespace workwear.Models.WearLk {
	public class UnansweredClaimsCounter {
		public UnansweredClaimsCounter(ToolButton button) {
			PangoText = new Pango.Layout(button.PangoContext);
			button.ExposeEvent += Button_ExposeEvent;
			this.button = button;
		}


		#region Отрисовка
		private int unansweredCount;
		public int UnansweredCount {
			get => unansweredCount; set {
				unansweredCount = value;
				PangoText.SetMarkup($"<span foreground=\"red\"><b>{unansweredCount}</b></span>");
			}
		}

		private Pango.Layout PangoText;
		private readonly ToolButton button;

		void Button_ExposeEvent(object o, ExposeEventArgs args) {
			int shift = 5;
			//Пробуем найти область с картинкой в случае если отображается текст и картинка одновременно.
			var image = ((button.Child as Button).Child as VBox)?.Children.First() as Image;
			if(image == null) {
				image = ((button.Child as Button).Child as Image); //Для случая когда только одна картинка.
				shift = 0;
			}

			if(image != null && UnansweredCount > 0) {
				Gdk.Rectangle targetRectangle = image.Allocation;
				PangoText.GetPixelSize(out int textWidth, out int textHeight);
				int x = targetRectangle.Right - textWidth - shift;
				int y = targetRectangle.Bottom - textHeight;
				Style.PaintLayout(button.Style, args.Event.Window, button.State, true, targetRectangle, button, null, x, y, PangoText);
			}
		}
		#endregion
	}
}
