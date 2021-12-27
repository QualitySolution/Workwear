using QS.DomainModel.Entity;

namespace workwear.Domain.Tools
{
	public class MessageTemplate: IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual string Name { get; set; }

		public virtual string MessageTitle { get; set; }

		public virtual string MessageText { get; set; }
	}
}