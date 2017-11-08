using System.ComponentModel.DataAnnotations;
using Gtk;
using QSOrmProject;
using QSOrmProject.Domain;

namespace workwear.Domain.Users
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "настройки пользователей",
		Nominative = "настройки пользователя")]
	public class UserSettings: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		User user;

		[Display (Name = "Пользователь")]
		public virtual User User {
			get { return user; }
			set { SetField (ref user, value, () => User); }
		}

		ToolbarStyle toolbarStyle = ToolbarStyle.Both;

		[Display (Name = "Стиль панели")]
		public virtual ToolbarStyle ToolbarStyle {
			get { return toolbarStyle; }
			set { SetField (ref toolbarStyle, value, () => ToolbarStyle); }
		}

		IconsSize toolBarIconsSize = IconsSize.Large;

		[Display (Name = "Размер иконок панели")]
		public virtual IconsSize ToolBarIconsSize {
			get { return toolBarIconsSize; }
			set { SetField (ref toolBarIconsSize, value, () => ToolBarIconsSize); }
		}

		private bool showToolbar;

		[Display(Name = "Показывать панель")]
		public virtual bool ShowToolbar
		{
			get { return showToolbar; }
			set { SetField(ref showToolbar, value, () => ShowToolbar); }
		}

		#endregion

		public UserSettings ()
		{

		}

		public UserSettings (User user)
		{
			User = user;
		}
	}

	public enum IconsSize
	{
		ExtraSmall,
		Small,
		Middle,
		Large
	}

	public class ToolBarIconsSizeStringType : NHibernate.Type.EnumStringType
	{
		public ToolBarIconsSizeStringType () : base (typeof(IconsSize))
		{
		}
	}

	public class ToolbarStyleStringType : NHibernate.Type.EnumStringType
	{
		public ToolbarStyleStringType () : base (typeof(ToolbarStyle))
		{
		}
	}

}

