using System;
using System.IO;
using QS.ViewModels;
using Workwear.Domain.Company;

namespace workwear.ViewModels.Company.EmployeeChildren
{
	public class EmployeePhotoViewModel : ViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly EmployeeViewModel employeeViewModel;

		public EmployeePhotoViewModel(EmployeeViewModel employeeViewModel)
		{
			this.employeeViewModel = employeeViewModel ?? throw new ArgumentNullException(nameof(employeeViewModel));
		}

		#region Свойства

		public string SuggestedPhotoName => Entity.FullName + ".jpg";

		public EmployeeCard Entity => employeeViewModel.Entity;

		public bool SensetiveSavePhoto => Entity.Photo != null;

		#endregion

		#region Действия

		public void SavePhoto(string fileName)
		{
			using(FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write)) {
				fs.Write(employeeViewModel.UoWGeneric.Root.Photo, 0, employeeViewModel.UoWGeneric.Root.Photo.Length);
			}
		}

		public void LoadPhoto(string fileName)
		{
			logger.Info("Загрузка фотографии...");

			using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
				if(fileName.ToLower().EndsWith(".jpg")) {
					using(MemoryStream ms = new MemoryStream()) {
						fs.CopyTo(ms);
						Entity.Photo = ms.ToArray();
					}
				}
				else {
					logger.Info("Конвертация в jpg ...");
					Gdk.Pixbuf image = new Gdk.Pixbuf(fs);
					Entity.Photo = image.SaveToBuffer("jpeg");
				}
			}
			OnPropertyChanged(nameof(SensetiveSavePhoto));
			logger.Info("Ok");
		}

		public void OpenPhoto()
		{
			string filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "temp_img.jpg");
			using(FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
				fs.Write(employeeViewModel.UoWGeneric.Root.Photo, 0, employeeViewModel.UoWGeneric.Root.Photo.Length);
			}
			System.Diagnostics.Process.Start(filePath);
		}

		#endregion
	}
}
