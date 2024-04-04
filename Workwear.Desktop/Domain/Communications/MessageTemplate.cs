using System.ComponentModel.DataAnnotations;
using System.Reflection;
using QS.DomainModel.Entity;

namespace Workwear.Domain.Communications
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "шаблоны",
		Nominative = "шаблон")]
	public class MessageTemplate : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private string name;
		[Required(ErrorMessage = "Название должно быть заполнено.")]
		[StringLength(100, ErrorMessage = "Название не может превышать 100 символов.")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		private string messageTitle;
		[Required(ErrorMessage = "Заголовок должен быть заполнен.")]
		[StringLength(200, ErrorMessage = "Заголовок сообщения не может превышать 200 символов.")]
		public virtual string MessageTitle {
			get { return messageTitle; }
			set { SetField(ref messageTitle, value, () => MessageTitle); }
		}

		private string messageText;
		[Required(ErrorMessage = "Сообщение не может быть пустым")]
		public virtual string MessageText {
			get { return messageText; }
			set { SetField(ref messageText, value, () => messageText); }
		}
		
		private string linkTitleText;
		[StringLength(100, ErrorMessage = "Заголовок ссылки не может превышать 100 символов.")]
		public virtual string LinkTitleText {
			get { return linkTitleText; }
			set { SetField(ref linkTitleText, value, () => linkTitleText); }
		}
		
		private string linkText;
		[StringLength(100, ErrorMessage = "Ссылка не может превышать 100 символов.")]
		public virtual string LinkText {
			get { return linkText; }
			set { SetField(ref linkText, value, () => linkText); }
		}
	}
}
