using System;
using Gtk;
using QS.Views;
using Workwear.ViewModels.Company.EmployeeChildren;

namespace workwear.Views.Company.EmployeeChildren
{
	public partial class EmployeePhotoView : ViewBase<EmployeePhotoViewModel>
	{
		public EmployeePhotoView(EmployeePhotoViewModel viewModel) : base(viewModel)
		{
			this.Build();

			yimagePhoto.Binding.AddBinding (ViewModel.Entity, e => e.Photo, w => w.ImageFile).InitializeFromSource ();
			buttonSavePhoto.Binding.AddBinding(ViewModel, v => v.SensetiveSavePhoto, w => w.Sensitive).InitializeFromSource();
		}

		protected void OnButtonLoadPhotoClicked(object sender, EventArgs e)
		{
			FileChooserDialog Chooser = new FileChooserDialog("Выберите фото для загрузки...",
			   (Gtk.Window)this.Toplevel,
				FileChooserAction.Open,
				"Отмена", ResponseType.Cancel,
				"Загрузить", ResponseType.Accept);

			FileFilter Filter = new FileFilter();
			Filter.AddPixbufFormats();
			Filter.Name = "Все изображения";
			Chooser.AddFilter(Filter);

			if((ResponseType)Chooser.Run() == ResponseType.Accept) {
				Chooser.Hide();
				ViewModel.LoadPhoto(Chooser.Filename);
			}
			Chooser.Destroy();
		}

		protected void OnButtonSavePhotoClicked(object sender, EventArgs e)
		{
			FileChooserDialog fc =
				new FileChooserDialog("Укажите файл для сохранения фотографии",
									   (Gtk.Window)this.Toplevel,
					FileChooserAction.Save,
					"Отмена", ResponseType.Cancel,
					"Сохранить", ResponseType.Accept);
			fc.CurrentName = ViewModel.SuggestedPhotoName;
			fc.Show();
			if(fc.Run() == (int)ResponseType.Accept) {
				fc.Hide();
				ViewModel.SavePhoto(fc.Filename);
			}
			fc.Destroy();
		}

		protected void OnYimagePhotoButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			if(args.Event.Type == Gdk.EventType.TwoButtonPress) {
				ViewModel.OpenPhoto();
			}
		}
	}
}
