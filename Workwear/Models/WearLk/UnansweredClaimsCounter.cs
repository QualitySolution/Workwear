using System;
using System.Linq;
using Gtk;
using QS.Cloud.WearLk.Client;

namespace workwear.Models.WearLk {
	public class UnansweredClaimsCounter {
		public UnansweredClaimsCounter(ToolButton button, ClaimsManagerService claimsService) {
			this.claimsService = claimsService ?? throw new ArgumentNullException(nameof(claimsService));
			PangoText = new Pango.Layout(button.PangoContext);
			this.button = button;
			button.ExposeEvent += Button_ExposeEvent;
			
			Connect();
		}

		#region Подписка на данные

		public void Connect()
		{
			claimsService.NeedForResponseCountChanged += ClaimsServiceOnNeedForResponseCountChanged;
			claimsService.SubscribeNeedForResponseCount();
		}

		private void ClaimsServiceOnNeedForResponseCountChanged(object sender, ReceiveNeedForResponseCountEventArgs e) {
			Application.Invoke( (s, arg) => UnansweredCount = e.Count);
		}

		#endregion

		#region Отрисовка
		private uint unansweredCount;
		public uint UnansweredCount {
			get => unansweredCount; set {
				unansweredCount = value;
				PangoText.SetMarkup($"<span foreground=\"red\"><b>{unansweredCount}</b></span>");
				button.QueueDraw();
			}
		}

		private Pango.Layout PangoText;
		private readonly ToolButton button;
		private readonly ClaimsManagerService claimsService;

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
