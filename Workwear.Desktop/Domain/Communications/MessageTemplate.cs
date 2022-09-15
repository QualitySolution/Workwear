using System.ComponentModel.DataAnnotations;
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
		[StringLength(200, ErrorMessage = "Заголовок не может превышать 200 символов.")]
		public virtual string MessageTitle {
			get { return messageTitle; }
			set { SetField(ref messageTitle, value, () => MessageTitle); }
		}

		private string messageText;
		[Required(ErrorMessage = "Сообщение не может быть пустым")]
		[StringLength(400, ErrorMessage = "Текст сообщения не может превышать 400 символов.")]
		public virtual string MessageText {
			get { return messageText; }
			set { SetField(ref messageText, value, () => messageText); }
		}
	}
}