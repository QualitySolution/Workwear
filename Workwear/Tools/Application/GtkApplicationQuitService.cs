namespace workwear.Tools.Application 
{
	public class GtkApplicationQuitService : QS.Project.Services.IApplicationQuitService 
	{
		public void Quit() 
		{
			Gtk.Application.Quit();
		}
	}
}
