using System.ComponentModel.DataAnnotations;
using Gtk;
using QS.DomainModel.Entity;
using QS.Project.Domain;

namespace workwear.Domain.Users
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "настройки пользователей",
		Nominative = "настройки пользователя")]
	public class UserSettings: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		UserBase user;

		[Display (Name = "Пользователь")]
		public virtual UserBase User {
			get { return user; }
			set { SetField (ref user, value, () => User); }
		}

		ToolbarStyle toolbarStyle = ToolbarStyle.Both;

		[Display (Name = "Стиль панели")]
		public virtual ToolbarStyle ToolbarStyle {
			get { return toolbarStyle; }
			set { SetField (ref toolbarStyle, value, () => ToolbarStyle); }
		}

		IconsSize toolBarIconsSize = IconsSize.Middle;

		[Display (Name = "Размер иконок панели")]
		public virtual IconsSize ToolBarIconsSize {
			get { return toolBarIconsSize; }
			set { SetField (ref toolBarIconsSize, value, () => ToolBarIconsSize); }
		}

		private bool showToolbar = true;

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

		public UserSettings (UserBase user)
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

