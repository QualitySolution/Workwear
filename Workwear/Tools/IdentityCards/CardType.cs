using System;
using QS.DomainModel.Entity;
using RglibInterop;

namespace workwear.Tools.IdentityCards
{
	public class CardType : PropertyChangedBase
	{
		public readonly RG_CARD_FAMILY_CODE CardTypeFamily;

		public CardType(RG_CARD_FAMILY_CODE cardTypeFamily)
		{
			this.CardTypeFamily = cardTypeFamily;
		}

		private bool active;
		public virtual bool Active {
			get => active;
			set => SetField(ref active, value);
		}

		public string Title {
			get {
				switch(CardTypeFamily) {
					case RG_CARD_FAMILY_CODE.CF_PINCODE:
						return "PIN код";
					case RG_CARD_FAMILY_CODE.CF_TEMIC:
						return "TEMIC";
					case RG_CARD_FAMILY_CODE.CF_HID:
						return "HID";
					case RG_CARD_FAMILY_CODE.CF_EMMARINE:
						return "Em-Marine";
					case RG_CARD_FAMILY_CODE.CF_INDALA:
						return "INDALA";
					case RG_CARD_FAMILY_CODE.CF_COTAG:
						return "COTAG";
					case RG_CARD_FAMILY_CODE.EF_MIFARE:
						return "Mifare";
					default:
						return "Неизвестный";
				};
			}
		}
	}
}
