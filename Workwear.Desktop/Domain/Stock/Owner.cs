using QS.DomainModel.Entity;

namespace Workwear.Domain.Stock 
{
	public class Owner : PropertyChangedBase, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; }

		private string name;
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private string description;
		public virtual string Description {
			get => description;
			set => SetField(ref description, value);
		}

		#endregion

	}
}
