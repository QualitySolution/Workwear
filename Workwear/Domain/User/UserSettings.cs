using System.ComponentModel.DataAnnotations;
using Gtk;
using QS.DomainModel.Entity;
using QS.Project.Domain;
using workwear.Domain.Company;
using workwear.Domain.Stock;

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

		private bool maximizeOnStart;
		[Display(Name = "Разворачивать окно при запуске")]
		public virtual bool MaximizeOnStart {
			get => maximizeOnStart;
			set => SetField(ref maximizeOnStart, value);
		}

		private Warehouse defaultWarehouse;

		[Display(Name = "Склад по умолчанию")]
		public virtual Warehouse DefaultWarehouse {
			get { return defaultWarehouse; }
			set { SetField(ref defaultWarehouse, value, () => DefaultWarehouse); }
		}

		private Organization defaultOrganization;

		[Display(Name = "Организация по умолчанию")]
		public virtual Organization DefaultOrganization {
			get { return defaultOrganization; }
			set { SetField(ref defaultOrganization, value, () => DefaultOrganization); }
		}

		private Leader defaultResponsiblePerson;

		[Display(Name = "Ответственное лицо по умолчанию")]
		public virtual Leader DefaultResponsiblePerson {
			get { return defaultResponsiblePerson; }
			set { SetField(ref defaultResponsiblePerson, value, () => DefaultResponsiblePerson); }
		}

		private Leader defaultLeader;

		[Display(Name = "Руководитель по умолчанию")]
		public virtual Leader DefaultLeader {
			get { return defaultLeader; }
			set { SetField(ref defaultLeader, value, () => DefaultLeader); }
		}
		#endregion

		#region Расчетные

		public virtual string Title => $"Настройки {User.Name}";

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

