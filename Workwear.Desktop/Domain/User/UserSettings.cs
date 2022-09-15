using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Project.Domain;
using Workwear.Domain.Company;
using Workwear.Domain.Stock;

namespace Workwear.Domain.Users
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
			get => user;
			set { SetField (ref user, value, () => User); }
		}

		ToolbarType toolbarStyle = ToolbarType.Both;
		[Display (Name = "Стиль панели")]
		public virtual ToolbarType ToolbarStyle {
			get => toolbarStyle;
			set { SetField (ref toolbarStyle, value, () => ToolbarStyle); }
		}

		IconsSize toolBarIconsSize = IconsSize.Middle;
		[Display (Name = "Размер иконок панели")]
		public virtual IconsSize ToolBarIconsSize {
			get => toolBarIconsSize;
			set { SetField (ref toolBarIconsSize, value, () => ToolBarIconsSize); }
		}

		private bool showToolbar = true;
		[Display(Name = "Показывать панель")]
		public virtual bool ShowToolbar
		{
			get => showToolbar;
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
			get => defaultWarehouse;
			set { SetField(ref defaultWarehouse, value, () => DefaultWarehouse); }
		}

		private Organization defaultOrganization;
		[Display(Name = "Организация по умолчанию")]
		public virtual Organization DefaultOrganization {
			get => defaultOrganization;
			set { SetField(ref defaultOrganization, value, () => DefaultOrganization); }
		}

		private Leader defaultResponsiblePerson;
		[Display(Name = "Ответственное лицо по умолчанию")]
		public virtual Leader DefaultResponsiblePerson {
			get => defaultResponsiblePerson;
			set { SetField(ref defaultResponsiblePerson, value, () => DefaultResponsiblePerson); }
		}

		private Leader defaultLeader;
		[Display(Name = "Руководитель по умолчанию")]
		public virtual Leader DefaultLeader {
			get => defaultLeader;
			set { SetField(ref defaultLeader, value, () => DefaultLeader); }
		}
		#endregion
		#region Расчетные
		public virtual string Title => $"Настройки {User.Name}";
		#endregion
		public UserSettings () { }
		public UserSettings (UserBase user) {
			User = user;
		}
	}
	public enum IconsSize {
		ExtraSmall,
		Small,
		Middle,
		Large
	}

	public enum ToolbarType {
		Both,
		Icons,
		Text,
	}
}
